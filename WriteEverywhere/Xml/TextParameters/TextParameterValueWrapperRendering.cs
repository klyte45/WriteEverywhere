extern alias TLM;
using System.Collections.Generic;
using WriteEverywhere.Font;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Rendering;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Xml
{
    internal static class TextParameterValueWrapperRendering
    {
        internal static BasicRenderInformation GetTargetText(this TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, BaseWriteOnXml instance, TextToWriteOnXml textDescriptor, DynamicSpriteFont targetFont, ushort refId, int secRefId, int tercRefId, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            string targetStr = "";
            var variableClass = CommandLevelSingleton.GetVariableClass(wrapper.m_varType);
            if (variableClass != null)
            {
                switch (instance)
                {
                    case OnNetInstanceCacheContainerXml cc:
                        targetStr = variableClass.GetTargetTextForNet(wrapper, cc, refId, secRefId, tercRefId, textDescriptor, out multipleOutput);
                        break;
                    case WriteOnBuildingPropXml bd:
                        targetStr = variableClass.GetTargetTextForBuilding(wrapper, propGroupDescriptor, bd, refId, secRefId, tercRefId, textDescriptor, out multipleOutput);
                        break;
                    case LayoutDescriptorVehicleXml ve:
                        targetStr = variableClass.GetTargetTextForVehicle(wrapper, refId, secRefId, tercRefId, textDescriptor, out multipleOutput);
                        break;
                    default:
                        multipleOutput = null;
                        break;
                }
            }
            else
            {
                multipleOutput = null;
            }
            return multipleOutput is null ? targetFont.DrawString(ModInstance.Controller, targetStr, default, FontServer.instance.ScaleEffective) : null;
        }


        public static string TryFormat(this TextParameterVariableWrapper wrapper, float value, float multiplier) => (value * multiplier).ToString(wrapper.paramContainer.numberFormat);
        public static string TryFormat(this TextParameterVariableWrapper wrapper, long value) => value.ToString(wrapper.paramContainer.numberFormat);
        public static string TryFormat(this TextParameterVariableWrapper wrapper, FormattableString value) => value.GetFormatted(wrapper.paramContainer.stringFormat);
    }
}
