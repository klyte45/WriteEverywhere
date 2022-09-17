using Klyte.Localization;

namespace WriteEverywhere.Xml
{
    public enum TextType
    {
        None,
        OwnName,
        Fixed,
        StreetPrefix,
        StreetSuffix,
        StreetNameComplete,
        Mileage,
        District,
        Park,
        DistrictOrPark,
        ParkOrDistrict,
        LinesSymbols,
        Direction,
        DistanceFromReference,
        LastStopLine,
        HwShield,
        NextStopLine,
        PrevStopLine,
        PlatformNumber,
        PostalCode,
        GameSprite,
        LineIdentifier,
        ParameterizedText,
        TimeTemperature,
        ParameterizedGameSprite,
        ParameterizedGameSpriteIndexed,
        LineFullName,
        CityName,
        HwCodeShort,
        HwCodeLong,
        HwDettachedPrefix,
        HwIdentifierSuffix
    }
    public enum TextContent
    {
        None,
        ParameterizedText,
        ParameterizedSpriteFolder,
        ParameterizedSpriteSingle,
        LinesSymbols,
        LinesNameList,
        HwShield,
        TimeTemperature,
        TextParameterSequence
    }
    public static class TextContentExtensions
    {
        public static string GetI18n(this TextContent content)
        {
            switch (content)
            {
                case TextContent.None: return Str.WTS_BOARD_TEXT_TYPE_DESC__None;
                case TextContent.ParameterizedText: return Str.WTS_BOARD_TEXT_TYPE_DESC__ParameterizedText;
                case TextContent.ParameterizedSpriteFolder: return Str.WTS_BOARD_TEXT_TYPE_DESC__ParameterizedSpriteFolder;
                case TextContent.ParameterizedSpriteSingle: return Str.WTS_BOARD_TEXT_TYPE_DESC__ParameterizedSpriteSingle;
                case TextContent.LinesSymbols: return Str.WTS_BOARD_TEXT_TYPE_DESC__LinesSymbols;
                case TextContent.LinesNameList: return Str.WTS_BOARD_TEXT_TYPE_DESC__LinesNameList;
                case TextContent.HwShield: return Str.WTS_BOARD_TEXT_TYPE_DESC__HwShield;
                case TextContent.TimeTemperature: return Str.WTS_BOARD_TEXT_TYPE_DESC__TimeTemperature;
                case TextContent.TextParameterSequence: return Str.WTS_BOARD_TEXT_TYPE_DESC__TextParameterSequence;
            }
            return content.ToString();
        }

    }
}