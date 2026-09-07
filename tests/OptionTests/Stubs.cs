using System;
using System.Collections.Generic;
namespace Colossal { public interface IDictionaryEntryError {} public interface IDictionarySource { IEnumerable<KeyValuePair<string,string>> ReadEntries(IList<IDictionaryEntryError> e, Dictionary<string,int> i); void Unload(); } }
namespace Colossal.IO.AssetDatabase { public class FileLocationAttribute : Attribute { public FileLocationAttribute(string s) {} } }
namespace Game.Modding {
 public interface IMod {}
 public class ModSetting { public ModSetting(IMod m) {} public int Saves; public void ApplyAndSave(){Saves++;} public virtual void SetDefaults() {} public string GetSettingsLocaleID()=>"Mod"; public string GetOptionTabLocaleID(string s)=>"Tab."+s; public string GetOptionGroupLocaleID(string s)=>"Group."+s; public string GetOptionLabelLocaleID(string s)=>"Label."+s; public string GetOptionDescLocaleID(string s)=>"Desc."+s; }
}
namespace Game.Settings {
 public class SettingsUIGroupOrderAttribute:Attribute { public SettingsUIGroupOrderAttribute(params string[] s) {} }
 public class SettingsUIShowGroupNameAttribute:Attribute { public SettingsUIShowGroupNameAttribute(params string[] s) {} }
 public class SettingsUISectionAttribute:Attribute { public SettingsUISectionAttribute(string a,string b) {} }
 public class SettingsUIButtonAttribute:Attribute {}
 public class SettingsUIMultilineTextAttribute:Attribute {}
}
namespace FirstMapPopup { public class Mod : Game.Modding.IMod { public const string DisplayName="NO MORE SIGNS"; } }
