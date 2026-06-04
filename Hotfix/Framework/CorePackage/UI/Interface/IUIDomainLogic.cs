namespace LccHotfix
{
    public interface IUIDomainLogic : IUILogic
    {
        void OnAddChildNode(ElementNode node);
        void OnRemoveChildNode(ElementNode node);
        bool OnRequireEscape(ElementNode node);
    }
}
