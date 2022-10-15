using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Plugins
{
    public delegate void CommandLevelValidate(TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer);

    public abstract class BaseCommandLevel
    {
        public Enum defaultValue;
        public string regexValidValues;
        public CommandLevel nextLevelByRegex;
        public Func<string> descriptionKey;

        public int level;
        public abstract IEnumerable<Enum> NextLevelsKeys { get; }
    }

    public class BaseCommandLevel<C> : BaseCommandLevel where C : BaseCommandLevel
    {
        public Dictionary<Enum, C> nextLevelOptions;


        public override IEnumerable<Enum> NextLevelsKeys => nextLevelOptions.Keys;

    }

    public class CommandLevel : BaseCommandLevel<CommandLevel>
    {
        public void ParseFormatting(string[] relativeParams, out VariableExtraParameterContainer extraParameterContainer)
        {
            extraParameterContainer = default;
            if (this == m_numberFormatFloat || this == m_numberFormatInt)
            {
                if (relativeParams.Length >= 1)
                {
                    try
                    {
                        var test = (this == m_numberFormatFloat ? 1f.ToString(relativeParams[0]) : 1.ToString(relativeParams[0]));
                        extraParameterContainer.numberFormat = relativeParams[0];
                    }
                    catch
                    {
                        extraParameterContainer.numberFormat = this == m_numberFormatFloat ? "0.0" : "0";
                    }
                    if (relativeParams.Length >= 2)
                    {
                        extraParameterContainer.prefix = relativeParams[1];
                        if (relativeParams.Length >= 3)
                        {
                            extraParameterContainer.suffix = relativeParams[2];
                        }
                    }
                }
            }
            if (this == m_stringFormat)
            {
                if (relativeParams.Length >= 1)
                {
                    extraParameterContainer.stringFormat = relativeParams[0];
                    if (relativeParams.Length >= 2)
                    {
                        extraParameterContainer.prefix = relativeParams[1];
                        if (relativeParams.Length >= 3)
                        {
                            extraParameterContainer.suffix = relativeParams[2];
                        }
                    }
                }
            }
            if (this == m_appendPrefix)
            {
                if (relativeParams.Length >= 1)
                {
                    extraParameterContainer.prefix = relativeParams[0];
                    if (relativeParams.Length >= 2)
                    {
                        extraParameterContainer.suffix = relativeParams[1];
                    }
                }
            }
            if (this == m_numberSet)
            {
                if (relativeParams.Length >= 1)
                {
                    extraParameterContainer.paramIdx = int.TryParse(relativeParams[0], out int param) ? param : -1;
                }
            }
        }


        public static string FromParameterPath(IEnumerable<string> path) => string.Join("", path.Select(x => Regex.Replace(x, @"([^\\])/|^/", "$1\\/") + "/").ToArray());

        public const string PROTOCOL_VARIABLE = "var://";

        public static readonly CommandLevel m_appendSuffix = new CommandLevel
        {
            descriptionKey = () => Str.WTS_PARAMVARS_DESC__COMMON_SUFFIX,
            regexValidValues = ".*",
            nextLevelByRegex = m_endLevel
        };
        public static readonly CommandLevel m_appendPrefix = new CommandLevel
        {
            descriptionKey = () => Str.WTS_PARAMVARS_DESC__COMMON_PREFIX,
            regexValidValues = ".*",
            nextLevelByRegex = m_appendSuffix
        };
        public static readonly CommandLevel m_numberFormatFloat = new CommandLevel
        {
            descriptionKey = () => Str.WTS_PARAMVARS_DESC__COMMON_NUMBERFORMAT_FLOAT,
            regexValidValues = ".*",
            nextLevelByRegex = m_appendPrefix
        };
        public static readonly CommandLevel m_numberFormatInt = new CommandLevel
        {
            descriptionKey = () => Str.WTS_PARAMVARS_DESC__COMMON_NUMBERFORMAT_INT,
            regexValidValues = ".*",
            nextLevelByRegex = m_appendPrefix
        };
        public static readonly CommandLevel m_stringFormat = new CommandLevel
        {
            descriptionKey = () => Str.WTS_PARAMVARS_DESC__COMMON_STRINGFORMAT,
            regexValidValues = "^[ULA]{0,2}$",
            nextLevelByRegex = m_appendPrefix
        };
        public static readonly CommandLevel m_numberSet = new CommandLevel
        {
            descriptionKey = () => Str.WTS_PARAMVARS_DESC__COMMON_PARAMNUM,
            regexValidValues = "^[0-9]{1,2}$",
            nextLevelByRegex = m_endLevel
        };
        public static readonly CommandLevel m_endLevel = new CommandLevel
        {
        };


    }
}
