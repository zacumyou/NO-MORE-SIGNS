using Colossal.Logging;
using Game;
using Game.Modding;
using Colossal.IO.AssetDatabase;
using Game.SceneFlow;
using Game.Rendering;
using Unity.Entities;
using Game.Effects;
namespace FirstMapPopup
{
    public sealed class Mod : IMod
    {
        public const string DisplayName = "NO MORE SIGNS";
        internal static PopupSettings Settings { get; private set; }
        internal static readonly ILog Log = LogManager.GetLogger("FirstMapPopup").SetShowsErrorsInUI(false);
        public void OnLoad(UpdateSystem updateSystem)
        {
            Settings = new PopupSettings(this);
            AssetDatabase.global.LoadSettings("NoMoreSigns", Settings, new PopupSettings(this));
            foreach (var language in SettingsLocale.Languages)
                GameManager.instance.localizationManager.AddSource(language, new SettingsLocale(Settings, language));
            Settings.RegisterInOptionsUI();
            updateSystem.UpdateAt<RoadObjectTrackingSystem>(SystemUpdatePhase.Modification5);
            updateSystem.UpdateBefore<RoadObjectRenderSystem, BatchInstanceSystem>(SystemUpdatePhase.Rendering);
            updateSystem.UpdateBefore<StreetLightFilterSystem, LightCullingSystem>(SystemUpdatePhase.Rendering);
            updateSystem.UpdateAfter<StreetLightRestoreSystem, LightCullingSystem>(SystemUpdatePhase.Rendering);
            Log.Info("FirstMapPopup loaded.");
        }
        public void OnDispose()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                world.GetExistingSystemManaged<StreetLightFilterSystem>()?.Restore();
                world.GetExistingSystemManaged<RoadObjectTrackingSystem>()?.ReleaseAll();
            }
            Settings?.UnregisterInOptionsUI();
            Settings = null;
            Log.Info("NO MORE SIGNS disposed.");
        }
    }
}
