namespace FirstMapPopup
{
    internal static class RoadVisibilityRules
    {
        // Scope: 1 = regular roads, 2 = highway rules, 3 = shared junction.
        internal static bool ShouldHide(int combinedMask, int scope, RoadObjectKind kind)
        {
            int bit = kind == RoadObjectKind.Sign ? 1 : kind == RoadObjectKind.TrafficLight ? 2
                : kind == RoadObjectKind.Prop ? 4 : kind == RoadObjectKind.StreetLight ? 8
                : kind == RoadObjectKind.SpeedMarking ? 16 : kind == RoadObjectKind.RoadArrow ? 32 : 0;
            if (scope == 0 || bit == 0) return false;
            return ((scope & 1) == 0 || (combinedMask & bit) != 0)
                && ((scope & 2) == 0 || ((combinedMask >> 6) & bit) != 0);
        }
    }
}
