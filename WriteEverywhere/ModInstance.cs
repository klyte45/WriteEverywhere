extern alias UUI;
using ColossalFramework;
using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Tools;
using WriteEverywhere.UI;

[assembly: AssemblyVersion("0.0.0.*")]
namespace WriteEverywhere
{
    public class ModInstance : BasicIUserMod<ModInstance, MainController>
    {
        public override string SimpleName { get; } = "Write Everywhere";
        public override string SafeName { get; } = "WriteEverywhere";
        public override string Description { get; } = Str.root_modDescription;
        public override float UIScale => Mathf.Clamp(UIScaleSaved.value, 0.5f, 2);

        public override string Acronym => "WE";

        public override Color ModColor => ColorExtensions.FromRGB("44aadd");

        protected override void SetLocaleCulture(CultureInfo culture) => Str.Culture = culture;

        public static readonly SavedInt StartTextureSizeFont = new SavedInt("K45_WE_startTextureSizeFont", Settings.gameSettingsFile, 0);
        public static readonly SavedInt FontQuality = new SavedInt("K45_WE_fontQuality", Settings.gameSettingsFile, 2);
        public static readonly SavedFloat ClockPrecision = new SavedFloat("K45_WE_clockPrecision", Settings.gameSettingsFile, 15);
        public static readonly SavedBool ClockShowLeadingZero = new SavedBool("K45_WE_clockShowLeadingZero", Settings.gameSettingsFile, true);
        public static readonly SavedBool Clock12hFormat = new SavedBool("K45_WE_clock12hFormat", Settings.gameSettingsFile, false);
        public static readonly SavedFloat UIScaleSaved = new SavedFloat("K45_WE_uiScale", Settings.gameSettingsFile, 1);
        public static readonly SavedFloat UIOpacitySaved = new SavedFloat("K45_WE_uiOpacity", Settings.gameSettingsFile, .85f);

        public override IUUIButtonContainerPlaceholder[] UUIButtons => new IUUIButtonContainerPlaceholder[]
        {
            new UUIToolButtonContainerPlaceholder(
                buttonName :  $"{SimpleName} - {Str.WTS_PICK_A_SEGMENT}",
                iconPath : "WTS_SegmentPickerIcon",
                tooltip : $"WE: {Str.WTS_PICK_A_SEGMENT}",
                toolGetter : ()=> ToolsModifierControl.toolController.GetComponent<SegmentEditorPickerTool>()
            ),
            new UUIWindowButtonContainerPlaceholder(
             buttonName: $"{SimpleName} - {Str.WTS_VEHICLEEDITOR_WINDOWTITLE}",
             tooltip: $"{SimpleName} - {Str.WTS_VEHICLEEDITOR_WINDOWTITLE}",
             iconPath: "WTS_VehicleEditorIcon",
             windowGetter: ()=>WTSVehicleLiteUI.Instance
             ),
        };

        public override void Group9SettingsUI(UIHelper group9)
        {
            base.Group9SettingsUI(group9);
            group9.AddButton(Str.we_settings_btnLabel, () =>
            {
                if (WESettingsGUI.Instance != null)
                {
                    WESettingsGUI.Instance.Visible = true;
                }
                else
                {
                    GameObjectUtils.CreateElement<WESettingsGUI>(UIView.GetAView().transform);
                }


            });
        }
    }
}
