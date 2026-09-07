using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
namespace FirstMapPopup
{
    [FileLocation("NoMoreSigns")]
    [SettingsUIGroupOrder("Reset", "RegularRoads", "Highways", "Actions", "About")]
    [SettingsUIShowGroupName("RegularRoads", "Highways")]
    public sealed class PopupSettings : ModSetting
    {
        public PopupSettings(IMod mod) : base(mod) { }
        [SettingsUISection("General", "Reset"), SettingsUIButton]
        public bool ResetToDefaults { set { SetDefaults(); ApplyAndSave(); } }

        // Existing property names preserve previously saved regular-road settings.
        [SettingsUISection("General", "RegularRoads")]
        public bool HideSigns { get; set; } = true;
        [SettingsUISection("General", "RegularRoads")]
        public bool HideTrafficLights { get; set; }
        [SettingsUISection("General", "RegularRoads")]
        public bool HideStreetLights { get; set; }
        [SettingsUISection("General", "RegularRoads")]
        public bool HideProps { get; set; } = true;
        [SettingsUISection("General", "RegularRoads")]
        public bool HideSpeedMarkings { get; set; }
        [SettingsUISection("General", "RegularRoads")]
        public bool HideRoadArrows { get; set; }

        [SettingsUISection("General", "Highways")]
        public bool HighwayHideSigns { get; set; } = true;
        [SettingsUISection("General", "Highways")]
        public bool HighwayHideTrafficLights { get; set; }
        [SettingsUISection("General", "Highways")]
        public bool HighwayHideStreetLights { get; set; }
        [SettingsUISection("General", "Highways")]
        public bool HighwayHideProps { get; set; } = true;
        [SettingsUISection("General", "Highways")]
        public bool HighwayHideSpeedMarkings { get; set; }
        [SettingsUISection("General", "Highways")]
        public bool HighwayHideRoadArrows { get; set; }

        [SettingsUISection("General", "Actions"), SettingsUIButton]
        public bool EnableAll { set { SetAll(true); ApplyAndSave(); } }
        [SettingsUISection("General", "Actions"), SettingsUIButton]
        public bool DisableAll { set { SetAll(false); ApplyAndSave(); } }
        [SettingsUISection("General", "About"), SettingsUIMultilineText]
        public string VersionInfo
        {
            get { var v = typeof(Mod).Assembly.GetName().Version; return $"{Mod.DisplayName} · v{v.Major}.{v.Minor}.{v.Build}"; }
        }
        internal bool AnyStreetLightsHidden() => HideStreetLights || HighwayHideStreetLights;
        internal int GetMask(bool highway) => highway
            ? Mask(HighwayHideSigns, HighwayHideTrafficLights, HighwayHideProps, HighwayHideStreetLights, HighwayHideSpeedMarkings, HighwayHideRoadArrows)
            : Mask(HideSigns, HideTrafficLights, HideProps, HideStreetLights, HideSpeedMarkings, HideRoadArrows);
        private static int Mask(bool signs, bool signals, bool props, bool lights, bool speeds, bool arrows) =>
            (signs ? 1 : 0) | (signals ? 2 : 0) | (props ? 4 : 0) | (lights ? 8 : 0) | (speeds ? 16 : 0) | (arrows ? 32 : 0);
        private void SetAll(bool value)
        {
            HideSigns = HideTrafficLights = HideProps = HideStreetLights = HideSpeedMarkings = HideRoadArrows = value;
            HighwayHideSigns = HighwayHideTrafficLights = HighwayHideProps = HighwayHideStreetLights = HighwayHideSpeedMarkings = HighwayHideRoadArrows = value;
        }
        public override void SetDefaults()
        {
            SetAll(false);
            HideSigns = HideProps = HighwayHideSigns = HighwayHideProps = true;
        }
    }
}
