using Godot;

namespace LccHotfix
{
    public interface IUIRoot
    {
        Node? RenderCamera { get; }
        Control Canvas { get; }
        Node Transform { get; }
        void Initialize();
        void Finalize();
        ElementNode? Find(string name);
        bool Find(ElementNode elementNode, out string? name);
        void Attach(string name, ElementNode elementNode);
        void Detach(ElementNode elementNode);
        UILayer GetLayerByID(UILayerID layerID);
    }
}
