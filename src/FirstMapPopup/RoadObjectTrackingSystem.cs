using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace FirstMapPopup
{
    // Runtime-only dictionaries: no custom ECS component, serializer or save payload.
    public partial class RoadObjectTrackingSystem : GameSystemBase
    {
        private EntityQuery _all, _changed;
        private readonly Dictionary<Entity, RoadObjectKind> _prefabs = new Dictionary<Entity, RoadObjectKind>();
        private readonly HashSet<Entity> _hidden = new HashSet<Entity>();
        private readonly HashSet<Entity> _hiddenStreetLights = new HashSet<Entity>();
        private struct Classification { public RoadObjectKind Kind; public int Scope; }
        private readonly Dictionary<Entity, Classification> _instances = new Dictionary<Entity, Classification>();
        private readonly List<Entity> _snapshot = new List<Entity>();
        private readonly HashSet<Entity> _members = new HashSet<Entity>();
        private bool _initialized;
        private int _cursor;
        private readonly List<Entity> _pending = new List<Entity>();
        private int _pendingCursor;
        private readonly HashSet<Entity> _queued = new HashSet<Entity>();
        private readonly System.Diagnostics.Stopwatch _budget = new System.Diagnostics.Stopwatch();
        private const int MaxObjectsPerUpdate = 256;
        private PrefabSystem _prefabSystem;
        private int _lastMask = -1;
        private int _refresh;

        internal int HiddenCount => _hidden.Count;

        internal bool IsHiddenStreetLightSource(Entity owner)
        {
            if (_hiddenStreetLights.Count == 0) return false;
            for (int depth = 0; depth < 32 && EntityManager.Exists(owner); depth++)
            {
                if (EntityManager.HasComponent<Deleted>(owner) || EntityManager.HasComponent<Temp>(owner)
                    || EntityManager.HasComponent<Game.Buildings.Building>(owner)) return false;
                if (_hiddenStreetLights.Contains(owner)) return true;
                if (!EntityManager.HasComponent<Owner>(owner)) return false;
                var next = EntityManager.GetComponentData<Owner>(owner).m_Owner;
                if (next == owner) return false;
                owner = next;
            }
            return false;
        }

        internal bool IsHidden(Entity entity) => _hidden.Contains(entity)
            && EntityManager.Exists(entity) && !EntityManager.HasComponent<Deleted>(entity);

        protected override void OnCreate()
        {
            base.OnCreate();
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            var required = new[] { ComponentType.ReadOnly<Owner>(), ComponentType.ReadOnly<PrefabRef>() };
            var excluded = new[] { ComponentType.ReadOnly<Deleted>(), ComponentType.ReadOnly<Temp>(),
                ComponentType.ReadOnly<Tree>(), ComponentType.ReadOnly<Game.Buildings.Building>(), ComponentType.ReadOnly<PrefabData>() };
            _all = GetEntityQuery(new EntityQueryDesc { All = required, None = excluded,
                Any = new[] { ComponentType.ReadOnly<Static>(), ComponentType.ReadOnly<Secondary>() } });
            _changed = GetEntityQuery(new EntityQueryDesc { All = required, None = excluded,
                Any = new[] { ComponentType.ReadOnly<Created>(), ComponentType.ReadOnly<Updated>() } });
        }

        protected override void OnGameLoaded(Context context)
        {
            base.OnGameLoaded(context);
            ReleaseAll();
            _prefabs.Clear();
            _lastMask = -1;
            _refresh = 0;
            _snapshot.Clear();
            _members.Clear();
            _initialized = false;
            _instances.Clear();
            _pending.Clear();
            _pendingCursor = 0;
            _queued.Clear();
            _cursor = 0;
        }

        protected override void OnUpdate()
        {
            var s = Mod.Settings;
            int mask = s == null ? 0 : s.GetMask(false) | (s.GetMask(true) << 6);
            if (GameManager.instance == null || GameManager.instance.gameMode != GameMode.Game) mask = 0;
            bool settingsChanged = mask != _lastMask;
            // Settings reuse classifications. Periodic reconciliation has the same budget.
            if (!_initialized || (_cursor >= _snapshot.Count && --_refresh <= 0))
            {
                _snapshot.Clear();
                _members.Clear();
                using (var all = _all.ToEntityArray(Allocator.Temp))
                    foreach (var entity in all)
                        if (_members.Add(entity)) _snapshot.Add(entity);
                // Include formerly hidden objects whose ownership/type has changed.
                foreach (var entity in _hidden)
                    if (_members.Add(entity)) _snapshot.Add(entity);
                _initialized = true;
                _instances.Clear();
                _prefabs.Clear();
                _cursor = 0;
                _refresh = 300;
                // Purge destroyed objects without issuing redundant render invalidations.
                _hidden.RemoveWhere(e => !EntityManager.Exists(e) || EntityManager.HasComponent<Deleted>(e));
                _hiddenStreetLights.RemoveWhere(e => !_hidden.Contains(e));
            }
            if (settingsChanged) _cursor = 0;
            using (var changed = _changed.ToEntityArray(Allocator.Temp))
                foreach (var entity in changed)
                {
                    if (_queued.Add(entity)) _pending.Add(entity);
                    if (_members.Add(entity)) _snapshot.Add(entity);
                }

            _budget.Restart();
            using (var commands = new EntityCommandBuffer(Allocator.Temp))
            {
                int processed = 0, transitions = 0;
                // Reserve half the count budget for the snapshot during construction.
                while (_pendingCursor < _pending.Count && processed < 2048 && transitions < MaxObjectsPerUpdate / 2
                    && _budget.Elapsed.TotalMilliseconds < 1)
                {
                    var entity = _pending[_pendingCursor++];
                    _queued.Remove(entity);
                    _instances.Remove(entity);
                    if (ApplyVisibility(entity, mask, commands)) transitions++;
                    processed++;
                }
                while (_cursor < _snapshot.Count && processed < 4096 && transitions < MaxObjectsPerUpdate
                    && _budget.Elapsed.TotalMilliseconds < 2)
                {
                    if (ApplyVisibility(_snapshot[_cursor++], mask, commands)) transitions++;
                    processed++;
                }
                commands.Playback(EntityManager);
            }
            if (_pendingCursor == _pending.Count)
            {
                _pending.Clear();
                _pendingCursor = 0;
            }
            _budget.Stop();
            if (settingsChanged) Mod.Log.Info($"Road hiding mask: {mask}; applying incrementally");
            _lastMask = mask;
        }

        private bool ApplyVisibility(Entity entity, int mask, EntityCommandBuffer commands)
        {
            if (!EntityManager.Exists(entity) || EntityManager.HasComponent<Deleted>(entity)
                || EntityManager.HasComponent<Temp>(entity) || !EntityManager.HasComponent<PrefabRef>(entity))
            {
                _hidden.Remove(entity);
                _hiddenStreetLights.Remove(entity);
                _instances.Remove(entity);
                return false;
            }
            if (!_instances.TryGetValue(entity, out var classification))
            {
                classification.Kind = ClassifyRoadInstance(entity, out classification.Scope);
                _instances[entity] = classification;
            }
            bool hide = RoadVisibilityRules.ShouldHide(mask, classification.Scope, classification.Kind);
            bool transition = hide ? _hidden.Add(entity) : _hidden.Remove(entity);
            if (hide && classification.Kind == RoadObjectKind.StreetLight) _hiddenStreetLights.Add(entity);
            else _hiddenStreetLights.Remove(entity);
            if (transition) QueueRenderRefresh(commands, entity);
            return transition;
        }

        private RoadObjectKind ClassifyRoadInstance(Entity entity, out int roadScope)
        {
            roadScope = 0;
            if (!EntityManager.HasComponent<Static>(entity) && !EntityManager.HasComponent<Secondary>(entity))
                return RoadObjectKind.None;
            var current = entity;
            var kind = RoadObjectKind.None;
            // Owner chains can include lanes, shared poles, and nested subobjects.
            for (int depth = 0; depth < 32 && EntityManager.Exists(current); depth++)
            {
                if (EntityManager.HasComponent<Deleted>(current) || EntityManager.HasComponent<Temp>(current)
                    || EntityManager.HasComponent<Game.Buildings.Building>(current)
                    || EntityManager.HasComponent<Tree>(current)) return RoadObjectKind.None;
                roadScope = GetRoadScope(current);
                if (roadScope != 0) return kind;
                if (EntityManager.HasComponent<PrefabRef>(current))
                {
                    var prefab = EntityManager.GetComponentData<PrefabRef>(current).m_Prefab;
                    if (depth == 0 && IsStructure(prefab)) return RoadObjectKind.None;
                    var parentKind = ClassifyPrefab(prefab, 0);
                    // Unknown decals must never inherit a sign/prop category from a parent.
                    if (depth == 0 && IsDecalOnly(prefab) && parentKind == RoadObjectKind.None)
                        return RoadObjectKind.None;
                    // A pole with signal heads belongs to the signal toggle, even if
                    // its mesh is a generic prop. Keep independent signs independent.
                    if (kind == RoadObjectKind.None || (kind == RoadObjectKind.Prop &&
                        (parentKind == RoadObjectKind.Sign || parentKind == RoadObjectKind.TrafficLight || parentKind == RoadObjectKind.StreetLight)))
                        kind = parentKind;
                }
                if (!EntityManager.HasComponent<Owner>(current)) return RoadObjectKind.None;
                var owner = EntityManager.GetComponentData<Owner>(current).m_Owner;
                if (owner == current) return RoadObjectKind.None;
                current = owner;
            }
            return RoadObjectKind.None;
        }

        private int GetRoadScope(Entity entity)
        {
            if (EntityManager.HasComponent<Node>(entity) && EntityManager.HasBuffer<ConnectedEdge>(entity))
            {
                int scope = 0;
                var edges = EntityManager.GetBuffer<ConnectedEdge>(entity, true);
                foreach (var edge in edges)
                    scope |= GetRoadEdgeScope(edge.m_Edge);
                return scope;
            }
            return GetRoadEdgeScope(entity);
        }

        private int GetRoadEdgeScope(Entity entity)
        {
            if (!EntityManager.Exists(entity) || EntityManager.HasComponent<Deleted>(entity)
                || EntityManager.HasComponent<Temp>(entity)
                || !EntityManager.HasComponent<Edge>(entity) || !EntityManager.HasComponent<PrefabRef>(entity)) return 0;
            var prefab = EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab;
            if (!EntityManager.HasComponent<RoadData>(prefab)) return 0;
            var flags = EntityManager.GetComponentData<RoadData>(prefab).m_Flags;
            return (flags & Game.Prefabs.RoadFlags.UseHighwayRules) != 0 ? 2 : 1;
        }

        private bool IsStructure(Entity prefab) => EntityManager.HasComponent<BuildingData>(prefab)
            || EntityManager.HasComponent<TreeData>(prefab) || EntityManager.HasComponent<PillarData>(prefab)
            || EntityManager.HasComponent<BridgeData>(prefab) || EntityManager.HasComponent<NetGeometryData>(prefab);

        private bool IsDecalOnly(Entity prefab)
        {
            if (!EntityManager.HasBuffer<SubMesh>(prefab)) return false;
            var meshes = EntityManager.GetBuffer<SubMesh>(prefab, true);
            bool found = false;
            foreach (var mesh in meshes)
            {
                if (!EntityManager.HasComponent<MeshData>(mesh.m_SubMesh)) continue;
                found = true;
                if ((EntityManager.GetComponentData<MeshData>(mesh.m_SubMesh).m_State & MeshFlags.Decal) == 0) return false;
            }
            return found;
        }

        private RoadObjectKind ClassifyPrefab(Entity prefab, int depth)
        {
            if (!EntityManager.Exists(prefab) || depth > 8) return RoadObjectKind.None;
            if (_prefabs.TryGetValue(prefab, out var known)) return known;
            if (IsStructure(prefab)) return RoadObjectKind.None;
            if (IsDecalOnly(prefab))
            {
                bool speed = EntityManager.HasComponent<TrafficSignData>(prefab)
                    && (EntityManager.GetComponentData<TrafficSignData>(prefab).m_TypeMask
                        & TrafficSignData.GetTypeMask(TrafficSignType.SpeedLimit)) != 0;
                var decalKind = RoadDecalRules.Classify(true, speed, EntityManager.HasComponent<LaneDirectionData>(prefab));
                _prefabs[prefab] = decalKind;
                return decalKind;
            }
            var kind = RoadObjectKind.None;
            if (EntityManager.HasComponent<TrafficLightData>(prefab)) kind = RoadObjectKind.TrafficLight;
            else if (EntityManager.HasComponent<StreetLightData>(prefab)) kind = RoadObjectKind.StreetLight;
            else if (EntityManager.HasComponent<TrafficSignData>(prefab)) kind = RoadObjectKind.Sign;
            else
            {
                // Detect composite sign/signal poles from their actual child prefabs.
                if (EntityManager.HasBuffer<Game.Prefabs.SubObject>(prefab))
                {
                    var children = EntityManager.GetBuffer<Game.Prefabs.SubObject>(prefab, true);
                    foreach (var child in children)
                    {
                        var childKind = ClassifyPrefab(child.m_Prefab, depth + 1);
                        if (childKind == RoadObjectKind.TrafficLight) { kind = childKind; break; }
                        if (childKind == RoadObjectKind.StreetLight) kind = childKind;
                        if (childKind == RoadObjectKind.Sign && kind == RoadObjectKind.None) kind = childKind;
                    }
                }
                if (kind == RoadObjectKind.None)
                {
                    // Hydrants/cabinets need not carry MeshFlags.Prop or utility data.
                    // This fallback still requires a road owner and excludes buildings/decals.
                    if (EntityManager.HasComponent<UtilityObjectData>(prefab)
                        || RoadPropNames.IsHydrantOrElectricalBox(_prefabSystem.GetPrefabName(prefab)))
                        kind = RoadObjectKind.Prop;
                    else if (EntityManager.HasBuffer<SubMesh>(prefab))
                    {
                        var meshes = EntityManager.GetBuffer<SubMesh>(prefab, true);
                        foreach (var mesh in meshes)
                            if (EntityManager.HasComponent<MeshData>(mesh.m_SubMesh))
                            {
                                var flags = EntityManager.GetComponentData<MeshData>(mesh.m_SubMesh).m_State;
                                if ((flags & MeshFlags.Prop) != 0 && (flags & MeshFlags.Decal) == 0)
                                { kind = RoadObjectKind.Prop; break; }
                            }
                    }
                }
            }
            _prefabs[prefab] = kind;
            return kind;
        }

        // Vanilla non-serialized render invalidation only; never Updated/Deleted/Hidden.
        private void QueueRenderRefresh(EntityCommandBuffer commands, Entity entity)
        {
            if (EntityManager.Exists(entity) && !EntityManager.HasComponent<Deleted>(entity)
                && !EntityManager.HasComponent<BatchesUpdated>(entity))
                commands.AddComponent<BatchesUpdated>(entity);
        }

        public void ReleaseAll()
        {
            if (_hidden.Count == 0 || World == null || !World.IsCreated) return;
            using (var commands = new EntityCommandBuffer(Allocator.Temp))
            {
                foreach (var entity in _hidden) QueueRenderRefresh(commands, entity);
                commands.Playback(EntityManager);
            }
            _hidden.Clear();
            _hiddenStreetLights.Clear();
        }
    }
}
