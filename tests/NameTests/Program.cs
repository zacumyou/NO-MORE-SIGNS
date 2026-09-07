using System;
using FirstMapPopup;
string[] yes = { "Hydrant", "Hydrant01", "Fire Hydrant 02", "EU Hydrant 01", "Electrical Box", "ElectricalBox01", "NA Electrical_Box 02", "Electric Box 01", "ElectricityBox02" };
string[] no = { null, "", "Street Light 01", "Traffic Light 01", "Electrical Substation", "HydrantDecorationShop", "Box", "Nonhydrant" };
foreach (var s in yes) if (!RoadPropNames.IsHydrantOrElectricalBox(s)) throw new Exception("Missed: " + s);
foreach (var s in no) if (RoadPropNames.IsHydrantOrElectricalBox(s)) throw new Exception("False positive: " + s);
Console.WriteLine("PASS: 17 asset-name classification cases.");
for (int bits = 0; bits < 8; bits++)
{
    bool decal = (bits & 1) != 0, speed = (bits & 2) != 0, arrow = (bits & 4) != 0;
    var expected = !decal ? RoadObjectKind.None : speed ? RoadObjectKind.SpeedMarking : arrow ? RoadObjectKind.RoadArrow : RoadObjectKind.None;
    if (RoadDecalRules.Classify(decal, speed, arrow) != expected) throw new Exception("Decal classification failed: " + bits);
}
Console.WriteLine("PASS: 8 decal cases, including non-decal signs and unrelated decals.");
