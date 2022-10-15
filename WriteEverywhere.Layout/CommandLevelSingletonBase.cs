using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using WriteEverywhere.Plugins;

namespace WriteEverywhere.Singleton
{
    public abstract class CommandLevelSingletonBase : MonoBehaviour
    {
        private Dictionary<Enum, RootCommandLevel> commandTree;
        public static CommandLevelSingletonBase Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            commandTree = ReadCommandTree();
        }

        protected abstract Dictionary<Enum, RootCommandLevel> ReadCommandTree();
        protected abstract string GetRootDescText();
        protected abstract string ValueToI18n(Enum value);
        public static string[] GetParameterPath(string input, out Enum refEnum)
        {
            var path = Regex.Split(input, @"(?<!\\)/").Select(x => x.Replace("\\/", "/")).ToArray();
            refEnum = Instance?.commandTree.Keys.Where(x => x.ToString() == path[0]).FirstOrDefault();
            return path;
        }

        public CommandLevel OnFilterParamByText(string inputText, out string currentLocaleDesc)
        {
            if ((inputText?.Length ?? 0) >= 4 && inputText.StartsWith(CommandLevel.PROTOCOL_VARIABLE))
            {
                var parameterPath = GetParameterPath(inputText.Substring(CommandLevel.PROTOCOL_VARIABLE.Length), out _);
                return IterateInCommandTree(out currentLocaleDesc, parameterPath, null, null, 0);
            }
            else
            {
                currentLocaleDesc = null;
                return null;
            }
        }


        private CommandLevel IterateInCommandTree(out string currentLocaleDesc, string[] parameterPath, Enum levelKey, CommandLevel currentLevel, int level)
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
                currentLocaleDesc = GetRootDescText();
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
                    ? ValueToI18n(currentLevel.defaultValue)
                    : ValueToI18n(levelKey);
            currentLevel.level = level;
            return currentLevel;
        }
    }
}