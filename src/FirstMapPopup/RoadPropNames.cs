using System.Text.RegularExpressions;

namespace FirstMapPopup
{
    internal static class RoadPropNames
    {
        // Match asset-name words (including numbered variants), not arbitrary substrings.
        private static readonly Regex TargetName = new Regex(
            @"(?:^|[^a-z])(?:fire[\s_-]*hydrant|hydrant|electrical[\s_-]*box|electric[\s_-]*box|electricity[\s_-]*box)(?=$|[^a-z])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        internal static bool IsHydrantOrElectricalBox(string name) =>
            !string.IsNullOrEmpty(name) && TargetName.IsMatch(name);
    }
}
