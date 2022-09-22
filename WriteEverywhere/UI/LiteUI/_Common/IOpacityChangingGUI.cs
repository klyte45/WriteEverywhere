using Kwytto.LiteUI;

namespace WriteEverywhere.UI
{
    public abstract class IOpacityChangingGUI : GUIRootWindowBase
    {
        public virtual void Awake()
        {
            BgOpacity = ModInstance.UIOpacitySaved.value;
        }
    }
}
