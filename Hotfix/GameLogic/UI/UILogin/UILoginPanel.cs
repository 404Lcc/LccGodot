using Godot;

namespace LccHotfix
{
    public class UILoginPanel : UIElementBase
    {
        public Button startBtn;

        public override void OnConstruct()
        {
            base.OnConstruct();

            IsFullScreen = true;
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);
        }

        public void OnStartBtn()
        {

        }
    }
}