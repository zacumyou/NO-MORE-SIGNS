namespace FirstMapPopup
{
    internal enum RoadObjectKind { None, Sign, TrafficLight, StreetLight, Prop, SpeedMarking, RoadArrow }

    internal static class RoadDecalRules
    {
        // Classification is independent of user language and prefab display names.
        // Road ownership is separately required by ClassifyRoadInstance before hiding.
        internal static RoadObjectKind Classify(bool isDecal, bool speedLimit, bool laneDirection)
        {
            if (!isDecal) return RoadObjectKind.None;
            if (speedLimit) return RoadObjectKind.SpeedMarking;
            if (laneDirection) return RoadObjectKind.RoadArrow;
            return RoadObjectKind.None;
        }
    }
}
