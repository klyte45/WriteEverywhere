using ColossalFramework;
using Kwytto.Data;
using System.Globalization;
using System.Xml.Serialization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Data
{
    [XmlRoot("EtcData")]
    public class WTSEtcData : DataExtensionBase<WTSEtcData>
    {
        public override string SaveId => "K45_WE_EtcData";

        [XmlElement("fontSettings")]
        public FontSettings FontSettings { get; set; } = new FontSettings();

        [XmlIgnore]
        private static readonly SavedInt m_temperatureUnit = new SavedInt(Settings.temperatureUnit, Settings.gameSettingsFile, DefaultSettings.temperatureUnit, true);
        [XmlIgnore]
        private static readonly SavedString m_formatEnvironmentName = new SavedString("K45_WE_Environment", Settings.gameSettingsFile, CultureInfo.CurrentCulture.ToString(), true);
        public static string FormatCultureString
        {
            get => m_formatEnvironmentName.value;
            set
            {
                m_formatEnvironmentName.value = value;
                try
                {
                    FormatCulture = CultureInfo.GetCultureInfo(value);
                }
                catch { }
            }
        }

        private static CultureInfo m_cachedCulture;
        public static CultureInfo FormatCulture
        {
            get
            {
                if (m_cachedCulture is null)
                {
                    try
                    {
                        m_cachedCulture = CultureInfo.GetCultureInfo(m_formatEnvironmentName);
                        if (m_cachedCulture.IsNeutralCulture)
                        {
                            m_cachedCulture = CultureInfo.GetCultureInfo("en-US");
                        }
                    }
                    catch
                    {
                        m_cachedCulture = CultureInfo.GetCultureInfo("en-US");
                    }
                }
                return m_cachedCulture;
            }
            set
            {
                if (!value.IsNeutralCulture)
                {
                    m_cachedCulture = value;
                    m_formatEnvironmentName.value = value.ToString();
                }
            }
        }

        internal static string FormatTemp(float num, string numFmt) => StringUtils.SafeFormat((m_temperatureUnit != 0) ? FormatFahrenheit(numFmt) : FormatCelsius(numFmt), (num * (m_temperatureUnit != 0 ? 1.8 : 1)) + (m_temperatureUnit != 0 ? 32 : 0));

        public static string FormatCelsius(string numFmt) => $"{{0:{numFmt}}}°C";
        public static string FormatFahrenheit(string numFmt) => $"{{0:{numFmt}}}°F";
    }
    public class FontSettings
    {
        private string m_publicTransportLineSymbolFont;

        [XmlAttribute("publicTransportFont")]
        public string PublicTransportLineSymbolFont
        {
            get => m_publicTransportLineSymbolFont; set
            {
                if (ModInstance.Controller is null || m_publicTransportLineSymbolFont != value)
                {
                    m_publicTransportLineSymbolFont = value;
                }
            }
        }

        [XmlAttribute("electronicFont")]
        public string ElectronicFont { get; set; }

        [XmlAttribute("stencilFont")]
        public string StencilFont { get; set; }

        [XmlAttribute("highwayShieldsFont")]
        public string HighwayShieldsFont { get; set; }

        internal string GetTargetFont(FontClass fontClass, bool allowNull = false)
        {
            switch (fontClass)
            {
                case FontClass.Regular:
                    return null;
                case FontClass.PublicTransport:
                    return m_publicTransportLineSymbolFont ?? (allowNull ? null : WEMainController.DEFAULT_FONT_KEY);
                case FontClass.ElectronicBoards:
                    return ElectronicFont ?? (allowNull ? null : WEMainController.DEFAULT_FONT_KEY);
                case FontClass.Stencil:
                    return StencilFont ?? (allowNull ? null : WEMainController.DEFAULT_FONT_KEY);
                case FontClass.HighwayShields:
                    return HighwayShieldsFont ?? (allowNull ? null : WEMainController.DEFAULT_FONT_KEY);
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
