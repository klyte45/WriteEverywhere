using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorIlluminationTab<F1, F2> : IGUITab<TextToWriteOnXml> where F1 : Enum, IConvertible where F2 : Enum
    {
        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Lamp);

        private readonly string[] m_materialTypes = EnumI18nExtensions.GetAllValuesI18n<MaterialType>();
        private readonly string[] m_blinkTypes = EnumI18nExtensions.GetAllValuesI18n<BlinkType>();
        private readonly F1[] m_flagsToSelect1
            = Enum.GetValues(typeof(F1))
            .Cast<F1>()
            .Where(x => Enum.GetName(typeof(F1), x) != null)
            .OrderBy(x => x.ToString())
            .ToArray();
        private readonly F2[] m_flagsToSelect2
            = Enum.GetValues(typeof(F2))
            .Cast<F2>()
            .Where(x => Enum.GetName(typeof(F2), x) != null)
            .OrderBy(x => x.ToString())
            .ToArray();
        private readonly GUIRootWindowBase m_root;


        public GeneralWritingEditorIlluminationTab(GUIColorPicker colorPicker) => m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();

        public bool DrawArea(Vector2 tabAreaSize, ref TextToWriteOnXml currentItem, int currentItemIdx, bool isEditable)
        {
            GUILayout.Label($"<i>{Str.WTS_TEXT_ILLUMINATION_ATTRIBUTES}</i>");
            var item = currentItem;

            if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_TEXT_MATERIALTYPE, (int)item.IlluminationConfig.IlluminationType, m_materialTypes, out var newVal, m_root, isEditable))
            {
                item.IlluminationConfig.IlluminationType = (MaterialType)newVal;
            }
            GUIKwyttoCommons.AddToggle(Str.we_textEditor_useAlsoForBg, ref item.IlluminationConfig.m_useForBg);
            if (item.IlluminationConfig.IlluminationType != MaterialType.OPAQUE)
            {

                GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_ILLUMINATIONSTRENGTH, ref item.IlluminationConfig.m_illuminationStrength, 0, 10, isEditable);
                if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_TEXT_BLINKTYPE, (int)item.IlluminationConfig.BlinkType, m_blinkTypes, out newVal, m_root, isEditable))
                {
                    item.IlluminationConfig.BlinkType = (BlinkType)newVal;
                }
                if (item.IlluminationConfig.BlinkType == BlinkType.Custom)
                {
                    GUIKwyttoCommons.AddVector4Field(tabAreaSize.x, item.IlluminationConfig.CustomBlink, Str.WTS_TEXT_CUSTOMBLINKPARAMS, Str.WTS_TEXT_CUSTOMBLINKPARAMS, isEditable);
                }
            }
            if (item.IlluminationConfig.IlluminationType == MaterialType.FLAGS)
            {
                GUILayout.Space(10);
                GUILayout.Label($"<color=#FFFF00>{Str.WTS_FLAGSREQUREDFORBIDDEN}</color>");
                var counter = 0;
                var itemsPerLine = Mathf.FloorToInt((tabAreaSize.x - 10) / 120);
                var width = (tabAreaSize.x - 10) / itemsPerLine;
                for (; counter < m_flagsToSelect1.Length + m_flagsToSelect2.Length;)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        for (int i = 0; i < itemsPerLine; i++)
                        {
                            if (counter < m_flagsToSelect1.Length)
                            {
                                var flag = m_flagsToSelect1[counter];
                                if (GUILayout.Button(flag.ToString(), IsRequired(item, flag) ? WEUIUtils.GreenButton : IsForbid(item, flag) ? WEUIUtils.RedButton : GUI.skin.button, GUILayout.Width(width)) && isEditable)
                                {
                                    ToggleFlag(item, flag);
                                }
                            }
                            else
                            {
                                var flag = m_flagsToSelect2[counter - m_flagsToSelect1.Length];
                                if (GUILayout.Button(flag.ToString(), IsRequired(item, flag) ? WEUIUtils.GreenButton : IsForbid(item, flag) ? WEUIUtils.RedButton : GUI.skin.button, GUILayout.Width(width)) && isEditable)
                                {
                                    ToggleFlag(item, flag);
                                }
                            }
                            counter++;
                            if (counter >= m_flagsToSelect1.Length + m_flagsToSelect2.Length)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return false;
        }
        private void ToggleFlag(TextToWriteOnXml item, F1 flag)
        {
            if (IsRequired(item, flag))
            {
                item.IlluminationConfig.m_requiredFlags &= ~(int)flag.ToUInt64();
                item.IlluminationConfig.m_forbiddenFlags |= (int)flag.ToUInt64();
            }
            else if (IsForbid(item, flag))
            {
                item.IlluminationConfig.m_forbiddenFlags &= ~(int)flag.ToUInt64();
            }
            else
            {
                item.IlluminationConfig.m_requiredFlags |= (int)flag.ToUInt64();
            }
        }

        private static bool IsForbid(TextToWriteOnXml item, F1 flag) => (item.IlluminationConfig.m_forbiddenFlags & (int)flag.ToUInt64()) > 0;
        private static bool IsRequired(TextToWriteOnXml item, F1 flag) => (item.IlluminationConfig.m_requiredFlags & (int)flag.ToUInt64()) > 0;

        private void ToggleFlag(TextToWriteOnXml item, F2 flag)
        {
            if (IsRequired(item, flag))
            {
                item.IlluminationConfig.m_requiredFlags2 &= ~(int)flag.ToUInt64();
                item.IlluminationConfig.m_forbiddenFlags2 |= (int)flag.ToUInt64();
            }
            else if (IsForbid(item, flag))
            {
                item.IlluminationConfig.m_forbiddenFlags2 &= ~(int)flag.ToUInt64();
            }
            else
            {
                item.IlluminationConfig.m_requiredFlags2 |= (int)flag.ToUInt64();
            }
        }

        private static bool IsForbid(TextToWriteOnXml item, F2 flag) => (item.IlluminationConfig.m_forbiddenFlags2 & (int)flag.ToUInt64()) > 0;
        private static bool IsRequired(TextToWriteOnXml item, F2 flag) => (item.IlluminationConfig.m_requiredFlags2 & (int)flag.ToUInt64()) > 0;

        public void Reset() { }
    }
}
