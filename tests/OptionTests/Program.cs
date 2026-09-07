using System;
using System.Linq;
using System.Collections.Generic;
using FirstMapPopup;
void Check(bool pass, string message) { if(!pass) throw new Exception(message); }
var s=new PopupSettings(new Mod());
Check(s.GetMask(false)==5 && s.GetMask(true)==5,"Defaults");
s.HideSigns=false; s.HighwayHideRoadArrows=true;
Check(s.GetMask(false)==4 && s.GetMask(true)==37,"Independent settings");
s.EnableAll=true; Check(s.GetMask(false)==63 && s.GetMask(true)==63 && s.Saves==1,"Enable all/save");
s.DisableAll=true; Check(s.GetMask(false)==0 && s.GetMask(true)==0 && s.Saves==2,"Disable all/save");
s.ResetToDefaults=true; Check(s.GetMask(false)==5 && s.GetMask(true)==5 && s.Saves==3,"Reset/save");
s.HighwayHideStreetLights=true; Check(s.AnyStreetLightsHidden(),"Highway lighting independent");
var kinds=new[]{RoadObjectKind.Sign,RoadObjectKind.TrafficLight,RoadObjectKind.Prop,RoadObjectKind.StreetLight,RoadObjectKind.SpeedMarking,RoadObjectKind.RoadArrow};
int cases=0;
for(int mask=0;mask<4096;mask++) for(int scope=0;scope<4;scope++) for(int k=0;k<6;k++) {
 bool expected=scope!=0 && (scope!=1 && scope!=3 || (mask&(1<<k))!=0) && (scope!=2 && scope!=3 || (mask&(1<<(k+6)))!=0);
 Check(RoadVisibilityRules.ShouldHide(mask,scope,kinds[k])==expected,"Scope isolation"); cases++;
}
HashSet<string> keys=null;
foreach(var language in SettingsLocale.Languages) {
 var entries=new SettingsLocale(s,language).ReadEntries(null,null).ToDictionary(x=>x.Key,x=>x.Value);
 Check(entries.Count==36 && entries.Values.All(x=>!string.IsNullOrWhiteSpace(x)),"Complete locale: "+language);
 if(keys==null)keys=entries.Keys.ToHashSet(); else Check(keys.SetEquals(entries.Keys),"Locale key parity");
}
Console.WriteLine($"PASS: {cases} road-scope cases; independent defaults; bulk actions/save calls; 6 locale catalogs (36 keys each). Engine APIs stubbed; runtime rendering/UI not exercised.");
