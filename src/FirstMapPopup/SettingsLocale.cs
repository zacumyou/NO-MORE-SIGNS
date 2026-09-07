using Colossal;
using System.Collections.Generic;
namespace FirstMapPopup
{
    public sealed class SettingsLocale : IDictionarySource
    {
        private readonly PopupSettings _settings;
        private readonly string _language;
        public SettingsLocale(PopupSettings settings, string language) { _settings = settings; _language = language; }
        internal static readonly string[] Languages = { "en-US", "ko-KR", "ja-JP", "zh-HANS", "zh-HANT", "es-ES" };
        // General, regular, highways, reset, enable all, disable all, version,
        // six category names, toggle description, reset description, enable/disable descriptions.
        private static readonly Dictionary<string, string[]> Texts = new Dictionary<string, string[]>
        {
            ["en-US"] = new[] { "Settings", "Regular roads (ON = hidden)", "Highways (ON = hidden)", "Reset to defaults", "Enable all", "Disable all", "Mod version",
                "Hide roadside signs", "Hide traffic lights", "Hide street lights and light sources", "Hide other road props (hydrants / electrical boxes)", "Hide speed-limit decals (Lane Marking Speed)", "Hide lane-direction decals (Road Arrow)",
                "ON hides this category on the selected road type; OFF restores it. Shared highway/regular junction objects require both switches ON. Only rendering changes; traffic rules and city-save data remain unchanged.",
                "Restore and save defaults for both road types: signs and other props ON; all other categories OFF.", "Turn ON all 12 hiding switches and save.", "Turn OFF all 12 hiding switches, restore normal rendering and save." },
            ["ko-KR"] = new[] { "설정", "일반도로 (ON = 숨김)", "고속도로 (ON = 숨김)", "기본값으로 설정", "전체 활성화", "전체 비활성화", "모드 버전",
                "표지판 숨기기", "신호등 숨기기", "가로등 숨기기 (광원 포함)", "기타 도로 프롭 숨기기 (소화전·전선함)", "속도제한 노면 표시 숨기기 (Lane Marking Speed)", "차로 방향 화살표 숨기기 (Road Arrow)",
                "ON: 선택한 도로 종류의 해당 대상을 숨깁니다. OFF: 기본 표시로 복원합니다. 고속도로·일반도로가 만나는 교차로의 공용 소품은 양쪽 설정이 ON일 때 숨깁니다. 렌더링만 변경하며 교통 규칙과 도시 저장 데이터는 유지합니다.",
                "두 도로 종류 모두 기본값으로 복원하고 저장합니다. 표지판·기타 프롭 ON, 나머지는 OFF입니다.", "12개 숨기기 토글을 모두 ON으로 바꾸고 저장합니다.", "12개 숨기기 토글을 모두 OFF로 바꿔 원래 표시로 복원하고 저장합니다." },
            ["ja-JP"] = new[] { "設定", "一般道路（ON = 非表示）", "高速道路（ON = 非表示）", "初期設定に戻す", "すべて有効にする", "すべて無効にする", "MODバージョン",
                "道路標識を非表示", "信号機を非表示", "街灯を非表示（光源を含む）", "その他の道路小物を非表示（消火栓・配電ボックス）", "速度制限の路面表示を非表示（Lane Marking Speed）", "車線方向の矢印を非表示（Road Arrow）",
                "ON: 選択した道路種別の対象を非表示にします。OFF: 通常の表示に戻します。高速道路と一般道路の接続部にある共通の小物は、両方がONの場合のみ非表示になります。描画のみを変更し、交通ルールや都市のセーブデータは変更しません。",
                "両方の道路種別を初期設定に戻して保存します。標識・その他の小物はON、それ以外はOFFです。", "12個の非表示設定をすべてONにして保存します。", "12個の非表示設定をすべてOFFにし、通常の表示に戻して保存します。" },
            ["zh-HANS"] = new[] { "设置", "普通道路（ON = 隐藏）", "高速公路（ON = 隐藏）", "恢复默认设置", "全部启用", "全部禁用", "模组版本",
                "隐藏道路标志", "隐藏交通信号灯", "隐藏路灯（包括光源）", "隐藏其他道路摆件（消防栓／配电箱）", "隐藏限速路面标记（Lane Marking Speed）", "隐藏车道方向箭头（Road Arrow）",
                "ON：隐藏所选道路类型的对应对象。OFF：恢复正常显示。高速公路与普通道路交汇处的共用摆件仅在两侧设置均为ON时隐藏。仅改变渲染，不修改交通规则或城市存档数据。",
                "恢复并保存两种道路的默认设置：道路标志和其他摆件为ON，其余为OFF。", "将全部12个隐藏开关设为ON并保存。", "将全部12个隐藏开关设为OFF，恢复正常显示并保存。" },
            ["zh-HANT"] = new[] { "設定", "一般道路（ON = 隱藏）", "高速公路（ON = 隱藏）", "恢復預設設定", "全部啟用", "全部停用", "模組版本",
                "隱藏道路標誌", "隱藏交通號誌", "隱藏路燈（包含光源）", "隱藏其他道路物件（消防栓／配電箱）", "隱藏速限路面標記（Lane Marking Speed）", "隱藏車道方向箭頭（Road Arrow）",
                "ON：隱藏所選道路類型的對應物件。OFF：恢復正常顯示。高速公路與一般道路交會處的共用物件，僅在兩側設定均為ON時隱藏。只改變繪製，不修改交通規則或城市存檔資料。",
                "恢復並儲存兩種道路的預設設定：道路標誌和其他物件為ON，其餘為OFF。", "將全部12個隱藏開關設為ON並儲存。", "將全部12個隱藏開關設為OFF，恢復正常顯示並儲存。" },
            ["es-ES"] = new[] { "Ajustes", "Carreteras normales (ON = ocultar)", "Autopistas (ON = ocultar)", "Restablecer valores predeterminados", "Activar todo", "Desactivar todo", "Versión del mod",
                "Ocultar señales de tráfico", "Ocultar semáforos", "Ocultar farolas y sus luces", "Ocultar otros objetos viales (hidrantes / cajas eléctricas)", "Ocultar marcas de límite de velocidad (Lane Marking Speed)", "Ocultar flechas de dirección (Road Arrow)",
                "ON oculta esta categoría en el tipo de vía seleccionado; OFF restaura su aspecto normal. Los objetos compartidos en cruces entre autopistas y carreteras normales requieren ambas opciones en ON. Solo cambia la representación visual, sin modificar las normas de tráfico ni los datos de la partida.",
                "Restablece y guarda los valores de ambos tipos de vía: señales y otros objetos en ON; el resto en OFF.", "Activa las 12 opciones de ocultación y guarda los ajustes.", "Desactiva las 12 opciones de ocultación, restaura la visualización normal y guarda los ajustes." }
        };
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            var t = Texts.TryGetValue(_language, out var text) ? text : Texts["en-US"];
            var entries = new Dictionary<string, string>
            {
                [_settings.GetSettingsLocaleID()] = Mod.DisplayName,
                [_settings.GetOptionTabLocaleID("General")] = t[0],
                [_settings.GetOptionGroupLocaleID("RegularRoads")] = t[1],
                [_settings.GetOptionGroupLocaleID("Highways")] = t[2]
            };
            Add(entries, nameof(PopupSettings.ResetToDefaults), t[3], t[14]);
            Add(entries, nameof(PopupSettings.EnableAll), t[4], t[15]);
            Add(entries, nameof(PopupSettings.DisableAll), t[5], t[16]);
            Add(entries, nameof(PopupSettings.VersionInfo), t[6], Mod.DisplayName);
            string[] properties = { "HideSigns", "HideTrafficLights", "HideStreetLights", "HideProps", "HideSpeedMarkings", "HideRoadArrows" };
            for (int i = 0; i < properties.Length; i++)
            {
                Add(entries, properties[i], t[7 + i], t[13]);
                Add(entries, "Highway" + properties[i], t[7 + i], t[13]);
            }
            return entries;
        }
        private void Add(Dictionary<string, string> entries, string property, string label, string description)
        {
            entries[_settings.GetOptionLabelLocaleID(property)] = label;
            entries[_settings.GetOptionDescLocaleID(property)] = description;
        }
        public void Unload() { }
    }
}
