using ColossalFramework;
using ColossalFramework.Globalization;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using SpriteFontPlus;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public class WESettingsDefaultFontsTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.we_settings_defaultFonts;
        public static readonly float[] m_qualityArray = new float[] { .5f, .75f, 1f, 1.25f, 1.5f, 2f, 4f, 8f };

        private Enum[] getterTypes = new Enum[]
        {
            TextRenderingClass.Buildings,
            TextRenderingClass.PlaceOnNet,
            TextRenderingClass.Vehicle,
            FontClass.PublicTransport          ,
            FontClass.ElectronicBoards         ,
            FontClass.Stencil                  ,
            FontClass.HighwayShields
        };

        private readonly Tuple<Enum, GUIFilterItemsScreen<State>>[] m_fontFilters;
        public WESettingsDefaultFontsTab()
        {
            m_fontFilters = new Tuple<Enum, GUIFilterItemsScreen<State>>[getterTypes.Length];
            for (int i = 0; i < getterTypes.Length; i++)
            {
                Enum enumVal = getterTypes[i];
                m_fontFilters[i] = Tuple.New(enumVal, new GUIFilterItemsScreen<State>(enumVal.ValueToI18n(), ModInstance.Controller, OnFilterParam, (x, y) => SetTo(enumVal, x, y), GoTo, State.Normal, (State)i, acceptsNull: true));

            }
        }

        private string GetFrom(Enum cat)
        {
            switch (cat)
            {
                case TextRenderingClass b:
                    return WTSEtcData.Instance.FontSettings.GetTargetFont(b);
                case FontClass c:
                    return WTSEtcData.Instance.FontSettings.GetTargetFont(c, true);
            }
            return null;
        }

        private void SetTo(Enum cat, int _, string y)
        {
            switch (cat)
            {
                case TextRenderingClass b:
                    WTSEtcData.Instance.FontSettings.SetTargetFont(b, y);
                    return;
                case FontClass c:
                    WTSEtcData.Instance.FontSettings.SetTargetFont(c, y);
                    return;
            }
        }

        public void DrawArea(Vector2 tabAreaSize)
        {
            switch (CurrentLocalState)
            {
                case State.Normal:
                    NormalDraw(tabAreaSize);
                    break;
                default:
                    m_fontFilters[(int)CurrentLocalState].Second.DrawSelectorView(tabAreaSize.y);
                    break;
            }

        }
        public void NormalDraw(Vector2 tabAreaSize)
        {
            if (!SimulationManager.exists || SimulationManager.instance.m_metaData is null)
            {
                GUILayout.Label(Str.we_settings_defaultFontsAvailableOnlyWhenCityLoaded);
                return;
            }
            using (new GUILayout.AreaScope(new Rect(default, tabAreaSize)))
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(Str.we_settings_defaultFontDescription);
                    foreach (var tuple in m_fontFilters)
                    {
                        tuple.Second.DrawButton(tabAreaSize.x, GetFrom(tuple.First));
                    }
                }
            }
        }

        private enum State
        {
            Normal = -1,
            GeneralFontPicker
        }
        private State CurrentLocalState { get; set; } = State.Normal;
        private void GoTo(State newState) => CurrentLocalState = newState;

        #region Search font

        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
        {
            setResult(FontServer.instance.GetAllFonts().Where(x => searchText.IsNullOrWhiteSpace() || LocaleManager.cultureInfo.CompareInfo.IndexOf(x, searchText, CompareOptions.IgnoreCase) >= 0).OrderBy(x => x).ToArray());
            yield return 0;
        }
        public void Reset()
        {
            CurrentLocalState = State.Normal;
        }
        #endregion
    }
}
