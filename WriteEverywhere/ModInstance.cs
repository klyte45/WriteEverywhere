extern alias UUI;
using ColossalFramework;
using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Tools;
using WriteEverywhere.UI;

[assembly: AssemblyVersion("0.0.0.10007")]
namespace WriteEverywhere
{
    public class ModInstance : BasicIUserMod<ModInstance, WEMainController>
    {
        public override string SimpleName { get; } = "Write Everywhere";
        public override string SafeName { get; } = "WriteEverywhere";
        public override string Description { get; } = Str.root_modDescription;

        public override string Acronym => "WE";

        public override Color ModColor => ColorExtensions.FromRGB("363d3b");

        protected override void SetLocaleCulture(CultureInfo culture) => Str.Culture = culture;

        public static readonly SavedInt StartTextureSizeFont = new SavedInt("K45_WE_startTextureSizeFont", Settings.gameSettingsFile, 0);
        public static readonly SavedInt FontQuality = new SavedInt("K45_WE_fontQuality", Settings.gameSettingsFile, 2);
        public static readonly SavedFloat ClockPrecision = new SavedFloat("K45_WE_clockPrecision", Settings.gameSettingsFile, 15);
        public static readonly SavedBool ClockShowLeadingZero = new SavedBool("K45_WE_clockShowLeadingZero", Settings.gameSettingsFile, true);
        public static readonly SavedBool Clock12hFormat = new SavedBool("K45_WE_clock12hFormat", Settings.gameSettingsFile, false);
        protected override Dictionary<string, Func<IBridgePrioritizable>> ModBridges { get; } = new Dictionary<string, Func<IBridgePrioritizable>>()
        {
            ["Vehicle Skins"] = () => controller?.ConnectorVS,
            ["Addresses"] = () => controller?.ConnectorADR,
            ["T. Lines Manager"] = () => controller?.ConnectorTLM,
        };

        private IUUIButtonContainerPlaceholder[] cachedUUI;
        public override IUUIButtonContainerPlaceholder[] UUIButtons => cachedUUI ?? (cachedUUI = new IUUIButtonContainerPlaceholder[]
        {
            new UUIToolButtonContainerPlaceholder(
                buttonName :  $"{SimpleName} - {Str.WTS_PICK_A_SEGMENT}",
                iconPath : "SegmentPickerIcon",
                tooltip : $"WE: {Str.WTS_PICK_A_SEGMENT}",
                toolGetter : ()=> ToolsModifierControl.toolController.GetComponent<SegmentEditorPickerTool>()
            ),
            new UUIWindowButtonContainerPlaceholder(
                buttonName :  $"{SimpleName} - {Str.we_buildingEditor_windowTitle}",
                tooltip : $"WE:  {Str.we_buildingEditor_windowTitle}",
             iconPath: "BuildingEditorIcon",
             windowGetter: ()=>BuildingLiteUI.Instance
             ),
            new UUIWindowButtonContainerPlaceholder(
             buttonName: $"{SimpleName} - {Str.WTS_VEHICLEEDITOR_WINDOWTITLE}",
             tooltip: $"{SimpleName} - {Str.WTS_VEHICLEEDITOR_WINDOWTITLE}",
             iconPath: "VehicleEditorIcon",
             windowGetter: ()=>WTSVehicleLiteUI.Instance
             ),
        });

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
