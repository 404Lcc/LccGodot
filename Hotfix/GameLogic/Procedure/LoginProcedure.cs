namespace LccHotfix
{
    [Procedure]
    public class LoginProcedure : LoadProcedureHandler
    {
        public LoginProcedure()
        {
            procedureType = ProcedureType.Login.ToInt();
            loadType = LoadingType.Fast;
        }

        public override void ProcedureStartHandler()
        {
            base.ProcedureStartHandler();
            //进入

            Log.Debug("进入login");

            Main.UIService.ShowDomain(UIRootDefine.UIRootLogin, UIPanelDefine.UILoginPanel);

            ProcedureLoadEndHandler();
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void ProcedureExitHandler()
        {
            base.ProcedureExitHandler();

            Log.Debug("退出login");
        }
    }
}