// Render filtering adapted from NoSpeedLimitMarkings (MIT).
// See THIRD-PARTY-NOTICES.txt for attribution and license.
using Game;
using Game.Rendering;
using Unity.Jobs;

namespace FirstMapPopup
{
    public partial class RoadObjectRenderSystem : GameSystemBase
    {
        private PreCullingSystem _preCulling;
        private RoadObjectTrackingSystem _tracking;

        protected override void OnCreate()
        {
            base.OnCreate();
            _preCulling = World.GetOrCreateSystemManaged<PreCullingSystem>();
            _tracking = World.GetOrCreateSystemManaged<RoadObjectTrackingSystem>();
        }

        protected override void OnUpdate()
        {
            if (Mod.Settings == null || _tracking.HiddenCount == 0) return;
            var data = _preCulling.GetUpdatedData(false, out JobHandle dependencies);
            dependencies.Complete();
            for (int i = 0; i < data.Length; i++)
            {
                var item = data[i];
                if (!_tracking.IsHidden(item.m_Entity)) continue;
                item.m_Flags &= ~PreCullingFlags.NearCamera;
                data[i] = item;
            }
            _preCulling.AddCullingDataWriter(default(JobHandle));
        }
    }
}
