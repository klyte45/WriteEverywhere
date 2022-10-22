using ColossalFramework;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Singleton
{
    public class CommandLevelSingleton : CommandLevelSingletonBase
    {
        private Dictionary<Enum, RootCommandLevel> commandTree;

        public static CommandLevelSingleton Instance { get; internal set; }

        private Dictionary<Enum, RootCommandLevel> ReadCommandTree()
        {
            var result = new Dictionary<Enum, RootCommandLevel>();
            var extensionsRegex = BridgeUtils.GetAllLoadableClassesInAppDomain<WEVariableExtensionRegex>();
            foreach (var clazz in extensionsRegex)
            {
                result[clazz.RootMenuEnumValueWithPrefix] = new RootCommandLevel
                {
                    SrcClass = clazz,
                    descriptionKey = () => clazz.RootMenuDescription,
                    regexValidValues = clazz.RegexValidValues,
                    nextLevelByRegex = clazz.NextLevelByRegex
                };
            }
            var extensionsEnum = BridgeUtils.GetAllLoadableClassesInAppDomain<WEVariableExtensionEnum>();
            foreach (var clazz in extensionsEnum)
            {
                result[clazz.RootMenuEnumValueWithPrefix] = new RootCommandLevel
                {
                    SrcClass = clazz,
                    descriptionKey = () => clazz.RootMenuDescription,
                    defaultValue = clazz.DefaultValue,
                    nextLevelOptions = clazz.CommandTree,
                };
            }

            return result;
        }
        protected override string GetRootDescText() => Str.WTS_PARAMVARS_DESC__VarLevelRoot;
        protected override string ValueToI18n(Enum value) => value.ValueToI18n();

        protected override void OnAwake()
        {
            Instance = this;
            commandTree = ReadCommandTree();
        }

        protected override IEnumerable<Enum> GetKeys() => commandTree.Keys;

        public override void ReadVariableData(TextRenderingClass clazz, string[] path, out Enum refEnum, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer)
        {
            refEnum = GetRootEnumNextLevelFor(path[0]);
            if (refEnum != default)
            {
                var cmd = commandTree[refEnum];
                cmd.SrcClass.Validate(clazz, path, ref type, ref subtype, ref index, out paramContainer);
            }
            else
            {
                paramContainer = default;
            }
        }

        public RootCommandLevel GetRootForPath(string[] path)
        {
            var refEnum = GetRootEnumNextLevelFor(path[0]);
            return refEnum != default ? commandTree[refEnum] : null;
        }

        public static WEVariableExtension GetVariableClass(Enum root) => root != null && ModInstance.Controller.CommandLevelSingleton.commandTree.TryGetValue(root, out var value) ? value.SrcClass : null;

        protected override Enum GetRootEnumNextLevelFor(string v) => commandTree.Keys.FirstOrDefault(x => GetEnumKeyValue(x, 0) == v);

        protected override BaseCommandLevel GetRootCommandLevel() => new BaseCommandLevel<RootCommandLevel>
        {
            nextLevelOptions = commandTree
        };

        protected override CommandLevel GetRootNextLevelFor(Enum e) => commandTree[e];

        protected override BaseCommandLevel IterateInCommandTree(out string currentLocaleDesc, string[] parameterPath)
        {
            var varType = GetRootEnumNextLevelFor(parameterPath[0]);
            if (varType != default)
            {
                return IterateInCommandTree(out currentLocaleDesc, parameterPath, varType, GetRootNextLevelFor(varType), 1, GetRootForPath(parameterPath));
            }
            else
            {
                currentLocaleDesc = GetRootDescText();
            }
            return GetRootCommandLevel();
        }
        private BaseCommandLevel IterateInCommandTree(out string currentLocaleDesc, string[] parameterPath, Enum levelKey, CommandLevel currentLevel, int level, RootCommandLevel root)
        {
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
                        return IterateInCommandTree(out currentLocaleDesc, parameterPath, varType, currentLevel.nextLevelOptions[varType], level + 1, root);
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
                                return IterateInCommandTree(out currentLocaleDesc, parameterPath, null, currentLevel.nextLevelByRegex, level + 1, root);
                            }
                        }
                    }
                }
            }
            currentLocaleDesc = !(currentLevel.descriptionKey is null)
                ? currentLevel.descriptionKey()
                : levelKey is null
                    ? root.SrcClass.GetSubvalueDescription(currentLevel.defaultValue)
                    : root.SrcClass.GetSubvalueDescription(levelKey);
            currentLevel.level = level;
            return currentLevel;
        }
    }
}