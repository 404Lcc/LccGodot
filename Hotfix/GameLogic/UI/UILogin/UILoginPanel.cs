using Godot;

namespace LccHotfix
{
    public class UILoginPanel : UIElementBase
    {
        public Button startBtn;

        public override void OnConstruct()
        {
            base.OnConstruct();

            LayerID = UILayerID.Main;
            IsFullScreen = true;
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);

            Log.Error("按钮是否存在" + (startBtn != null));
            startBtn.Pressed += OnStartBtn;
        }

        public override object OnHide()
        {
            startBtn.Pressed -= OnStartBtn;

            return base.OnHide();
        }

        public void OnStartBtn()
        {
            Log.Error("点击按钮");
        }
    }
}