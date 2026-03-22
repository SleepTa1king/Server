using UIFramework.Window;
using Utils;

public class StartDemoSiganl:ASignal
{

}

public class StartWindowController : WindowController
{
    public void UI_Start()
    {
        Signals.Get<StartDemoSiganl>().Dispatch();
    }
}

