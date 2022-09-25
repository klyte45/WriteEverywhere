using ColossalFramework;
using Kwytto.Data;
using System.Xml.Serialization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Data
{
    [XmlRoot("WTSEtcData")]
    public class WTSEtcData : DataExtensionBase<WTSEtcData>
    {
        public override string SaveId => "K45_WE_EtcData";

        [XmlElement("fontSettings")]
        public FontSettings FontSettings { get; set; } = new FontSettings();

        [XmlIgnore]
        private static readonly SavedInt m_temperatureUnit = new SavedInt(Settings.temperatureUnit, Settings.gameSettingsFile, DefaultSettings.temperatureUnit, true);

        internal static string FormatTemp(float num) => StringUtils.SafeFormat((m_temperatureUnit != 0) ? kFormatFahrenheit : kFormatCelsius, (num * (m_temperatureUnit != 0 ? 1.8 : 1)) + (m_temperatureUnit != 0 ? 32 : 0));

        public const string kFormatCelsius = "{0:0}°C";
        public const string kFormatFahrenheit = "{0:0}°F";
    }
    public class FontSettings
    {
        private string m_publicTransportLineSymbolFont;
        private string highwayShieldsFont;

        [XmlAttribute("publicTransportFont")]
        public string PublicTransportLineSymbolFont
        {
            get => m_publicTransportLineSymbolFont; set
            {
                if (ModInstance.Controller is null || m_publicTransportLineSymbolFont != value)
                {
                    m_publicTransportLineSymbolFont = value;
                    ModInstance.Controller?.AtlasesLibrary?.PurgeAllLines();
                }
            }
        }

        [XmlAttribute("electronicFont")]
        public string ElectronicFont { get; set; }

        [XmlAttribute("stencilFont")]
        public string StencilFont { get; set; }

        [XmlAttribute("highwayShieldsFont")]
        public string HighwayShieldsFont
        {
            get => highwayShieldsFont; set
            {
                {
                    if (ModInstance.Controller is null || highwayShieldsFont != value)
                    {
                        highwayShieldsFont = value;
                        if (LoadingManager.instance.m_loadingComplete)
                        {
                            ModInstance.Controller?.HighwayShieldsAtlasLibrary?.PurgeShields();
                        }
                    }
                }
            }
        }

        internal string GetTargetFont(FontClass fontClass, bool allowNull = false)
        {
            switch (fontClass)
            {
                case FontClass.Regular:
                    return null;
                case FontClass.PublicTransport:
                    return m_publicTransportLineSymbolFont ?? (allowNull ? null : MainController.DEFAULT_FONT_KEY);
                case FontClass.ElectronicBoards:
                    return ElectronicFont ?? (allowNull ? null : MainController.DEFAULT_FONT_KEY);
                case FontClass.Stencil:
                    return StencilFont ?? (allowNull ? null : MainController.DEFAULT_FONT_KEY);
                case FontClass.HighwayShields:
                    return HighwayShieldsFont ?? (allowNull ? null : MainController.DEFAULT_FONT_KEY);
            }
            return null;
        }

        internal void SetTargetFont(FontClass fontClass, string newVal)
        {
            switch (fontClass)
            {
                case FontClass.PublicTransport:
                    m_publicTransportLineSymbolFont = newVal;
                    break;
                case FontClass.ElectronicBoards:
                    ElectronicFont = newVal;
                    break;
                case FontClass.Stencil:
                    StencilFont = newVal;
                    break;
                case FontClass.HighwayShields:
                    HighwayShieldsFont = newVal;
                    break;
            }
        }
        internal string GetTargetFont(TextRenderingClass renderingClass)
        {
            switch (renderingClass)
            {
                case TextRenderingClass.Buildings:
                    return WTSBuildingData.Instance.DefaultFont;
                case TextRenderingClass.PlaceOnNet:
                    return WTSOnNetData.Instance.DefaultFont;
                case TextRenderingClass.Vehicle:
                    return WTSVehicleData.Instance.DefaultFont;
            }
            return null;
        }
        internal void SetTargetFont(TextRenderingClass renderingClass, string newVal)
        {
            switch (renderingClass)
            {
                case TextRenderingClass.Buildings:
                    WTSBuildingData.Instance.DefaultFont = newVal;
                    return;
                case TextRenderingClass.PlaceOnNet:
                    WTSOnNetData.Instance.DefaultFont = newVal;
                    return;
                case TextRenderingClass.Vehicle:
                    WTSVehicleData.Instance.DefaultFont = newVal;
                    return;
            }
        }
    }
}
