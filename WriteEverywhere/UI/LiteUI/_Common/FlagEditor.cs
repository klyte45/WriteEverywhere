using Kwytto.LiteUI;
using System;
using UnityEngine;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public class FlagEditor<F1, F2> where F1 : Enum, IConvertible where F2 : Enum, IConvertible
    {
        public void DrawFlagsEditor(float totalWidth, F1[] flagsToSelect1, F2[] flagsToSelect2, FlaggedSettings item, bool isEditable)
        {
            var itemsPerLine = Mathf.FloorToInt(totalWidth / 180);
            var width = (totalWidth - (6 * itemsPerLine)) / itemsPerLine;
            DrawFlagsEditor(flagsToSelect1, flagsToSelect2, width, item, itemsPerLine, isEditable);
        }

        private void DrawFlagsEditor(F1[] flagsToSelect1, F2[] flagsToSelect2, float width, FlaggedSettings item, int itemsPerLine, bool isEditable)
        {
            var counter = 0;
            for (; counter < flagsToSelect1.Length + flagsToSelect2.Length;)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int i = 0; i < itemsPerLine; i++)
                    {
                        if (counter < flagsToSelect1.Length)
                        {
                            var flag = flagsToSelect1[counter];
                            if (GUILayout.Button(flag.ToString(), IsRequired(item, flag) ? WEUIUtils.GreenButton : IsForbid(item, flag) ? WEUIUtils.RedButton : GUI.skin.button, GUILayout.Width(width)) && isEditable)
                            {
                                ToggleFlag(item, flag);
                            }
                        }
                        else
                        {
                            var flag = flagsToSelect2[counter - flagsToSelect1.Length];
                            if (GUILayout.Button(flag.ToString(), IsRequired(item, flag) ? WEUIUtils.GreenButton : IsForbid(item, flag) ? WEUIUtils.RedButton : GUI.skin.button, GUILayout.Width(width)) && isEditable)
                            {
                                ToggleFlag(item, flag);
                            }
                        }
                        counter++;
                        if (counter >= flagsToSelect1.Length + flagsToSelect2.Length)
                        {
                            break;
                        }
                    }
                }
            }

        }

        private void ToggleFlag(FlaggedSettings item, F1 flag)
        {
            if (IsRequired(item, flag))
            {
                item.m_requiredFlags &= ~(int)flag.ToUInt64();
                item.m_forbiddenFlags |= (int)flag.ToUInt64();
            }
            else if (IsForbid(item, flag))
            {
                item.m_forbiddenFlags &= ~(int)flag.ToUInt64();
            }
            else
            {
                item.m_requiredFlags |= (int)flag.ToUInt64();
            }
        }

        private static bool IsForbid(FlaggedSettings item, F1 flag) => (item.m_forbiddenFlags & (int)flag.ToUInt64()) > 0;
        private static bool IsRequired(FlaggedSettings item, F1 flag) => (item.m_requiredFlags & (int)flag.ToUInt64()) > 0;

        private void ToggleFlag(FlaggedSettings item, F2 flag)
        {
            if (IsRequired(item, flag))
            {
                item.m_requiredFlags2 &= ~(int)flag.ToUInt64();
                item.m_forbiddenFlags2 |= (int)flag.ToUInt64();
            }
            else if (IsForbid(item, flag))
            {
                item.m_forbiddenFlags2 &= ~(int)flag.ToUInt64();
            }
            else
            {
                item.m_requiredFlags2 |= (int)flag.ToUInt64();
            }
        }

        private static bool IsForbid(FlaggedSettings item, F2 flag) => (item.m_forbiddenFlags2 & (int)flag.ToUInt64()) > 0;
        private static bool IsRequired(FlaggedSettings item, F2 flag) => (item.m_requiredFlags2 & (int)flag.ToUInt64()) > 0;
    }
}
