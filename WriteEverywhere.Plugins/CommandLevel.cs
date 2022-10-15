﻿using ColossalFramework;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Plugins
{
    public delegate void CommandLevelValidate(TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer);
    public class RootCommandLevel : CommandLevel
    {
        public CommandLevelValidate Validate { get; internal set; }

    }


    public class CommandLevel
    {

        public Enum defaultValue;
        public Dictionary<Enum, CommandLevel> nextLevelOptions;
        public string regexValidValues;
        public CommandLevel nextLevelByRegex;
        public Func<string> descriptionKey;

        public int level;

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

        public static string[] GetParameterPath(string input, out Enum refEnum)
        {
            var path = Regex.Split(input, @"(?<!\\)/").Select(x => x.Replace("\\/", "/")).ToArray();
            refEnum = commandTree.Keys.Where(x => x.ToString() == path[0]).FirstOrDefault();
            return path;
        }

        public static string FromParameterPath(IEnumerable<string> path) => string.Join("/", path.Select(x => Regex.Replace(x, @"([^\\])/|^/", "$1\\/")).ToArray()) + "/";

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
        private static readonly Dictionary<Enum, RootCommandLevel> commandTree = ReadCommandTree();

        private static Dictionary<Enum, RootCommandLevel> ReadCommandTree()
        {
            var result = new Dictionary<Enum, RootCommandLevel>();
            foreach (var value in Enum.GetValues(typeof(VariableType)).Cast<VariableType>())
            {
                if (value == 0)
                {
                    continue;
                }

                result[value] = value.GetCommandTree();
            }
            WEVariableExtension[] extensions = BridgeUtils.GetAllLoadableClassesInAppDomain<WEVariableExtension>();
            foreach (var clazz in extensions)
            {
                result[clazz.RootMenuEnumValueWithPrefix] = new RootCommandLevel
                {
                    Validate = clazz.Validate,
                    descriptionKey = () => clazz.RootMenuDescription,
                    defaultValue = clazz.DefaultValue,
                    nextLevelOptions = clazz.AccessibleSubmenusEnum.ToDictionary(x => x, x => clazz.GetCommandTree(x))
                };
            }

            return result;
        }

        public static CommandLevel OnFilterParamByText(string inputText, out string currentLocaleDesc)
        {
            if ((inputText?.Length ?? 0) >= 4 && inputText.StartsWith(PROTOCOL_VARIABLE))
            {
                var parameterPath = GetParameterPath(inputText.Substring(PROTOCOL_VARIABLE.Length), out _);
                return IterateInCommandTree(out currentLocaleDesc, parameterPath, null, null, 0);
            }
            else
            {
                currentLocaleDesc = null;
                return null;
            }
        }

        private static CommandLevel IterateInCommandTree(out string currentLocaleDesc, string[] parameterPath, Enum levelKey, CommandLevel currentLevel, int level)
        {
            if (currentLevel is null)
            {
                if (level < parameterPath.Length - 1)
                {
                    var validKey = commandTree.Keys.Where(x => x.ToString() == parameterPath[level]).FirstOrDefault();
                    if (validKey != default)
                    {
                        return IterateInCommandTree(out currentLocaleDesc, parameterPath, validKey, currentLevel.nextLevelOptions[validKey], level + 1);
                    }
                }
                currentLocaleDesc = Str.WTS_PARAMVARS_DESC__VarLevelRoot;
                return null;
            }
            else if (level < parameterPath.Length - 1)
            {
                if (currentLevel.defaultValue != null)
                {
                    var varType = currentLevel.defaultValue;
                    try
                    {
                        varType = (Enum)Enum.Parse(varType.GetType(), parameterPath[level]);
                    }
                    catch
                    {

                    }
                    if (varType != currentLevel.defaultValue && currentLevel.nextLevelOptions.ContainsKey(varType))
                    {
                        return IterateInCommandTree(out currentLocaleDesc, parameterPath, varType, currentLevel.nextLevelOptions[varType], level + 1);
                    }
                }
                else
                {
                    if (!currentLevel.regexValidValues.IsNullOrWhiteSpace())
                    {
                        if (Regex.IsMatch(parameterPath[level], $"^{currentLevel.regexValidValues}$"))
                        {
                            if (currentLevel.nextLevelByRegex != null)
                            {
                                return IterateInCommandTree(out currentLocaleDesc, parameterPath, null, currentLevel.nextLevelByRegex, level + 1);
                            }
                        }
                    }
                }
            }
            currentLocaleDesc = !(currentLevel.descriptionKey is null)
                ? currentLevel.descriptionKey()
                : levelKey is null
                    ? currentLevel.defaultValue?.ValueToI18n()
                    : levelKey.ValueToI18n();
            currentLevel.level = level;
            return currentLevel;
        }

    }
}