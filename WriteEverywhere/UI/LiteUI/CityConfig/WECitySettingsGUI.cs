﻿using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    internal class WECitySettingsGUI : GUIOpacityChanging
    {

        public static WECitySettingsGUI Instance { get; private set; }
        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init(Str.we_citySettings_title, new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(440, 260));
            var tabs = new IGUIVerticalITab[] {
                        new WESettingsDefaultFontsTab(),
                        new WESettingsFoldersGeneralTab(),
                    };
            m_tabsContainer = new GUIVerticalTabsContainer(tabs);
            Visible = false;
        }
        protected override bool showOverModals => false;
        protected override float FontSizeMultiplier => .9f;

        protected override bool requireModal => false;

        private GUIVerticalTabsContainer m_tabsContainer;


        protected override void DrawWindow(Vector2 size)
        {
            m_tabsContainer.DrawListTabs2(new Rect(default, size), 200);
        }
        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            m_tabsContainer?.Reset();
        }

        protected override void OnWindowDestroyed()
        {
            Instance = null;
        }
    }
}
