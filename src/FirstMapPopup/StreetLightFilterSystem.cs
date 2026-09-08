using Game;
using Game.Effects;
using Game.SceneFlow;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

namespace FirstMapPopup
{
    // Bracket only LightCullingSystem. Its job copies visible lights into its own
    // output, so restoring the input afterwards does not bring hidden lights back.
    public partial class StreetLightFilterSystem : GameSystemBase
    {
        private EffectControlSystem _effects;
        private RoadObjectTrackingSystem _tracking;
        private readonly List<OriginalLight> _originals = new List<OriginalLight>();

        private struct OriginalLight
        {
            public int Index;
            public Entity Owner, Prefab;
            public int EffectIndex;
            public float Intensity;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _effects = World.GetOrCreateSystemManaged<EffectControlSystem>();
            _tracking = World.GetOrCreateSystemManaged<RoadObjectTrackingSystem>();
        }

        protected override void OnUpdate()
        {
            // Defensive recovery if a previous frame did not reach the restore system.
            Restore();
            if (Mod.Settings == null || GameManager.instance == null
                || GameManager.instance.gameMode != GameMode.Game || _tracking.HiddenCount == 0) return;
            var data = _effects.GetEnabledData(false, out JobHandle dependencies);
            dependencies.Complete();
            for (int i = 0; i < data.Length; i++)
            {
                var item = data[i];
                if ((item.m_Flags & (EnabledEffectFlags.IsLight | EnabledEffectFlags.IsEnabled))
                    != (EnabledEffectFlags.IsLight | EnabledEffectFlags.IsEnabled)
                    || item.m_Intensity == 0f || !_tracking.IsHiddenStreetLightSource(item.m_Owner)) continue;
                _originals.Add(new OriginalLight { Index = i, Owner = item.m_Owner,
                    Prefab = item.m_Prefab, EffectIndex = item.m_EffectIndex, Intensity = item.m_Intensity });
                // EnabledEffectData is a runtime NativeList entry, not an ECS/save component.
                item.m_Intensity = 0f;
                data[i] = item;
            }
            _effects.AddEnabledDataWriter(default(JobHandle));
        }

        internal void Restore()
        {
            if (_originals.Count == 0 || _effects == null) return;
            var data = _effects.GetEnabledData(false, out JobHandle dependencies);
            // Wait for LightCullingJob's registered reader before restoring the input.
            dependencies.Complete();
            foreach (var original in _originals)
            {
                if (original.Index >= data.Length) continue;
                var item = data[original.Index];
                // Never restore by a recycled list index or overwrite another changed value.
                if (item.m_Owner != original.Owner || item.m_Prefab != original.Prefab
                    || item.m_EffectIndex != original.EffectIndex || item.m_Intensity != 0f) continue;
                item.m_Intensity = original.Intensity;
                data[original.Index] = item;
            }
            _effects.AddEnabledDataWriter(default(JobHandle));
            _originals.Clear();
        }
    }

    public partial class StreetLightRestoreSystem : GameSystemBase
    {
        private StreetLightFilterSystem _filter;
        protected override void OnCreate()
        {
            base.OnCreate();
            _filter = World.GetOrCreateSystemManaged<StreetLightFilterSystem>();
        }
        protected override void OnUpdate() => _filter.Restore();
    }
}
