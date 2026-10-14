namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgWindowChecks
{
    public static void ReorgWindowStartsInNew()
    {
        var item = ReorgWindow.NewReorgWindow("code-926", "name-984");
        Check.Equal(item.Status, ReorgWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgWindowMovesToRunning()
    {
        var item = ReorgWindow.NewReorgWindow("code-926", "name-984");
        try
        {
            item.MoveTo(ReorgWindowStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgWindowStatus.Running, "status");
    }

    public static void ReorgWindowRejectsMovesFromFailed()
    {
        var item = ReorgWindow.NewReorgWindow("code-926", "name-984");
        item.Status = ReorgWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(ReorgWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        ReorgWindowStartsInNew();
        ReorgWindowMovesToRunning();
        ReorgWindowRejectsMovesFromFailed();
    }
}
