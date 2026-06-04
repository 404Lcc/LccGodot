using LccGodot.Services.UI.Node;

namespace LccGodot.Services.UI.Root;

public interface IUIRoot
{
    void Initialize();

    void FinalizeRoot();

    ElementNode? Find(string name);

    void Attach(string name, ElementNode elementNode);

    void Detach(ElementNode elementNode);

    UILayer GetLayerById(UILayerId layerId);
}
