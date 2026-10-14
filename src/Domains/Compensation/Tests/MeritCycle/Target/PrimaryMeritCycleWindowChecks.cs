namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PrimaryMeritCycleWindowChecks
{
    public static void PrimaryMeritCycleWindowStartsInNew()
    {
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("code-880", "name-186");
        Check.Equal(item.Status, PrimaryMeritCycleWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryMeritCycleWindowMovesToQueued()
    {
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("code-880", "name-186");
        try
        {
            item.MoveTo(PrimaryMeritCycleWindowStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryMeritCycleWindowStatus.Queued, "status");
    }

    public static void PrimaryMeritCycleWindowRejectsMovesFromFailed()
    {
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("code-880", "name-186");
        item.Status = PrimaryMeritCycleWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(PrimaryMeritCycleWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleWindowStartsInNew();
        PrimaryMeritCycleWindowMovesToQueued();
        PrimaryMeritCycleWindowRejectsMovesFromFailed();
    }
}
