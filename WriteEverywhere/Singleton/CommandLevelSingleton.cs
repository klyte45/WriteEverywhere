using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;

namespace WriteEverywhere.Singleton
{
    public class CommandLevelSingleton : CommandLevelSingletonBase
    {
        protected override Dictionary<Enum, RootCommandLevel> ReadCommandTree()
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
        protected override string GetRootDescText() => Str.WTS_PARAMVARS_DESC__VarLevelRoot;
        protected override string ValueToI18n(Enum value) => value.ValueToI18n();
    }
}