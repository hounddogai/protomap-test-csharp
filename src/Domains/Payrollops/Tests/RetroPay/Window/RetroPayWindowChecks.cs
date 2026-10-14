namespace Acme.Workforce.Payrollops.Tests.RetroPay.Window;

using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RetroPayWindowChecks
{
    public static void RetroPayWindowStartsInIdle()
    {
        var item = RetroPayWindow.NewRetroPayWindow("code-111", "label-582");
        Check.Equal(item.Status, RetroPayWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayWindowMovesToWarming()
    {
        var item = RetroPayWindow.NewRetroPayWindow("code-111", "label-582");
        try
        {
            item.MoveTo(RetroPayWindowStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayWindowStatus.Warming, "status");
    }

    public static void RetroPayWindowRejectsMovesFromStopped()
    {
        var item = RetroPayWindow.NewRetroPayWindow("code-111", "label-582");
        item.Status = RetroPayWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(RetroPayWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RetroPayWindowStartsInIdle();
        RetroPayWindowMovesToWarming();
        RetroPayWindowRejectsMovesFromStopped();
    }
}
