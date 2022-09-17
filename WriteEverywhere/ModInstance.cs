extern alias UUI;

using ColossalFramework;
using Klyte.Localization;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using WriteEverywhere.Tools;

[assembly: AssemblyVersion("0.0.0.*")]
namespace WriteEverywhere
{
    public class ModInstance : BasicIUserMod<ModInstance, MainController>
    {
        public override string SimpleName { get; } = "Write Everywhere";

        public override string Description { get; } = Str.root_modDescription;

        public override string Acronym => "WE";

        public override Color ModColor => ColorExtensions.FromRGB("44aadd");

        protected override void SetLocaleCulture(CultureInfo culture) => Str.Culture = culture;

        public static readonly SavedInt StartTextureSizeFont = new SavedInt("K45_WTS_startTextureSizeFont", Settings.gameSettingsFile, 0);
        public static readonly SavedInt FontQuality = new SavedInt("K45_WTS_fontQuality", Settings.gameSettingsFile, 2);
        public static readonly SavedFloat ClockPrecision = new SavedFloat("K45_WTS_clockPrecision", Settings.gameSettingsFile, 15);
        public static readonly SavedBool ClockShowLeadingZero = new SavedBool("K45_WTS_clockShowLeadingZero", Settings.gameSettingsFile, true);
        public static readonly SavedBool Clock12hFormat = new SavedBool("K45_WTS_clock12hFormat", Settings.gameSettingsFile, false);

        public override IUUIButtonContainerPlaceholder[] UUIButtons => new[]
        {
            new UUIToolButtonContainerPlaceholder(
                buttonName :  $"{SimpleName} - {Str.WTS_PICK_A_SEGMENT}",
                iconPath : $"UI.Images.K45_WTS_SegmentPickerIcon.png",
                tooltip : $"WE: {Str.WTS_PICK_A_SEGMENT}",
                toolGetter : ()=> ToolsModifierControl.toolController.GetComponent<SegmentEditorPickerTool>()
            )
        };
    }
}
