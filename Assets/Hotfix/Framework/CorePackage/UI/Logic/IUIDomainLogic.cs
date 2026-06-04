using LccGodot.Services.UI.Node;

namespace LccGodot.Services.UI.Logic;

public interface IUIDomainLogic : IUILogic
{
    void OnAddChildNode(ElementNode node);

    void OnRemoveChildNode(ElementNode node);

    bool OnRequireEscape(ElementNode node);
}
