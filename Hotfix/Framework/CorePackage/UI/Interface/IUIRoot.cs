using Godot;

namespace LccHotfix
{
    /// <summary>
    /// UI渲染的根元素，概念上等于一张逻辑画布
    /// </summary>
    public interface IUIRoot
    {
        /// <summary>
        /// UIRoot的相机节点，Godot UI通常不依赖它渲染
        /// </summary>
        public Camera2D RenderCamera { get; }

        /// <summary>
        /// UIRoot用的画布节点
        /// </summary>
        public Control Canvas { get; }

        /// <summary>
        /// UIRoot的布局
        /// </summary>
        public Node Transform { get; }

        /// <summary>
        /// 初始化一张画布，进入可以渲染元素的状态
        /// </summary>
        public void Initialize();

        /// <summary>
        /// 销毁一张画布，取消其上所有元素的渲染状态
        /// </summary>
        public void Finalize();

        public ElementNode Find(string name);
        public bool Find(ElementNode elementNode, out string name);
        public void Attach(string name, ElementNode elementNode);
        public void Detach(ElementNode elementNode);
        public UILayer GetLayerByID(UILayerID layerID);
    }
}
