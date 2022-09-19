using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    internal class WESettingsGUI : GUIRootWindowBase
    {
        public static WESettingsGUI Instance { get; private set; }
        private GUIVerticalTabsContainer m_tabsContainer;



        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            Instance = this;
            requireModal = true;
            Init(Str.we_settings_windowTitle, new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(440, 260));
            var tabs = new IGUIVerticalITab[] {
                new WESettingsUITab(),
                new WESettingsFoldersGeneralTab(),
                new WESettingsQualityTab(this),
                new WESettingsClockTab(this)
            };
            m_tabsContainer = new GUIVerticalTabsContainer(tabs);
        }

        public void Start() => Visible = false;

        public void Update()
        {
        }
        protected override void DrawWindow()
        {
            m_tabsContainer.DrawListTabs(new Rect(0, 25, WindowRect.width, WindowRect.height - 25), 200);
        }
        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            m_tabsContainer?.Reset();
        }
    }
}
