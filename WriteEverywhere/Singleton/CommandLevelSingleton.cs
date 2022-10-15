using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static WEVariableExtension GetVariableClass(Enum root) => root != null && ModInstance.Controller.CommandLevelSingleton.commandTree.TryGetValue(root, out var value) ? value.SrcClass : null;

        protected override Enum GetRootEnumNextLevelFor(string v) => commandTree.Keys.FirstOrDefault(x => GetEnumKeyValue(x, -1) == v);

        protected override BaseCommandLevel GetRootCommandLevel() => new BaseCommandLevel<RootCommandLevel>
        {
            nextLevelOptions = commandTree
        };

        protected override CommandLevel GetRootNextLevelFor(Enum e) => commandTree[e];
    }
}