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
                    var clazz = ReflectionUtils.GetInterfaceImplementations(typeof(CommandLevelSingletonBase)).First();
                    m_instance = BasicIUserMod.Instance.OwnGO.AddComponent(clazz) as CommandLevelSingletonBase;
                    LogUtils.DoWarnLog($"Creating command level singleton of type: {clazz} ({m_instance})");
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


        protected abstract BaseCommandLevel IterateInCommandTree(out string currentLocaleDesc, string[] parameterPath);


        protected abstract BaseCommandLevel GetRootCommandLevel();
        protected abstract Enum GetRootEnumNextLevelFor(string v);
        protected abstract CommandLevel GetRootNextLevelFor(Enum e);

        public static string GetEnumKeyValue(Enum input, int level)
        {
            return level > 0 || input is VariableType ? input.ToString() : $"{input.GetType().Name}::{input}";
        }

    }
}