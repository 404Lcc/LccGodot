namespace LccHotfix
{
    public class DefaultProcedureHelper : IProcedureHelper
    {
        public void UpdateLoadingTime(LoadProcedureHandler handler)
        {
            if (handler.IsLoading)
            {

            }
        }

        public void ResetSpeed()
        {
        }

        public void UnloadAllPanel(LoadProcedureHandler last, LoadProcedureHandler cur)
        {
            Main.UIService.HideAllDomain();
            
            Main.UIService.ForceClearReleaseQueue();
        }

        public void OpenChangeProcedurePanel(LoadProcedureHandler handler)
        {
            if (handler == null || handler.turnNode == null)
                return;

            TurnNode node = handler.turnNode;
            if (string.IsNullOrEmpty(node.nodeName))
                return;


            if (node.nodeType == NodeType.Domain)
            {
                Main.UIService.ShowDomain(node.nodeName, node.nodeParam);
            }
            else
            {
                Main.UIService.ShowElement(node.nodeName, node.nodeParam);
            }

            handler.turnNode = null;
        }

        public void ShowProcedureLoading(LoadingType loadType)
        {
            switch (loadType)
            {
                case LoadingType.Normal:
                    break;
                case LoadingType.Fast:
                    break;
            }
        }
    }
}