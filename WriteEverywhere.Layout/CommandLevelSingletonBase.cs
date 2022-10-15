using ColossalFramework;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using WriteEverywhere.Plugins;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
{

    public abstract class CommandLevelSingletonBase : MonoBehaviour
    {
        protected static CommandLevelSingletonBase m_instance;
        internal static CommandLevelSingletonBase Instance
        {
            get
            {
                if (m_instance is null)
                {
                    m_instance = BasicIUserMod.Instance.OwnGO.AddComponent(ReflectionUtils.GetInterfaceImplementations(typeof(CommandLevelSingletonBase)).First()) as CommandLevelSingletonBase;
                }
                return m_instance;
            }
        }


        protected void Awake()
        {
            OnAwake();
        }
        protected abstract void OnAwake();
        protected abstract IEnumerable<Enum> GetKeys();
        public abstract void ReadVariableData(TextRenderingClass clazz, string[] path, out Enum refEnum, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer);
        protected abstract string GetRootDescText();
        protected abstract string ValueToI18n(Enum value);

        public static string[] GetParameterPath(string input)
        {
            return Regex.Split(input, @"(?<!\\)/").Select(x => x.Replace("\\/", "/")).ToArray();
        }

        public BaseCommandLevel OnFilterParamByText(string inputText, out string currentLocaleDesc)
        {
            if (inputText?.StartsWith(CommandLevel.PROTOCOL_VARIABLE) ?? false)
            {
                var parameterPath = GetParameterPath(inputText.Substring(CommandLevel.PROTOCOL_VARIABLE.Length));
                return IterateInCommandTree(out currentLocaleDesc, parameterPath);
            }
            else
            {
                currentLocaleDesc = null;
                return null;
            }
        }


        private BaseCommandLevel IterateInCommandTree(out string currentLocaleDesc, string[] parameterPath, Enum levelKey = null, CommandLevel currentLevel = null, int level = 0)
        {
            if (currentLevel is null || level < 1)
            {
                var varType = GetRootEnumNextLevelFor(parameterPath[0]);
                if (varType != default)
                {
                    return IterateInCommandTree(out currentLocaleDesc, parameterPath, varType, GetRootNextLevelFor(varType), 1);
                }
                else
                {
                    currentLocaleDesc = GetRootDescText();
                }
                return GetRootCommandLevel();
            }
            if (level < parameterPath.Length - 1)
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
                    ? ValueToI18n(currentLevel.defaultValue)
                    : ValueToI18n(levelKey);
            currentLevel.level = level;
            return currentLevel;
        }

        protected abstract BaseCommandLevel GetRootCommandLevel();
        protected abstract Enum GetRootEnumNextLevelFor(string v);
        protected abstract CommandLevel GetRootNextLevelFor(Enum e);

        public static string GetEnumKeyValue(Enum input, int level)
        {
            return level >= 0 || input is VariableType ? input.ToString() : $"{input.GetType().Name}::{input}";
        }

    }
}