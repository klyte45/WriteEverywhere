using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    internal class WESettingsGUI : GUIOpacityChanging
    {
        protected override float FontSizeMultiplier => .9f;
        public static WESettingsGUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObjectUtils.CreateElement<WESettingsGUI>(UIView.GetAView().transform);
                    instance.Init(Str.we_settings_windowTitle, new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(440, 260));
                    var tabs = new IGUIVerticalITab[] {
                        new WESettingsFoldersGeneralTab(),
                        new WESettingsQualityTab(instance),
                        new WESettingsClockTab(instance),
                        new WESettingsDefaultFontsTab()
                    };
                    instance.m_tabsContainer = new GUIVerticalTabsContainer(tabs);
                }
                return instance;
            }
        }
        protected override bool showOverModals => true;

        protected override bool requireModal => true;

        private GUIVerticalTabsContainer m_tabsContainer;
        private static WESettingsGUI instance;


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
            instance = null;
        }
    }
}
