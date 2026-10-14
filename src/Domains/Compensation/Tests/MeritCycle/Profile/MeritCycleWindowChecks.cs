namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleWindowChecks
{
    public static void MeritCycleWindowStartsInNew()
    {
        var item = MeritCycleWindow.NewMeritCycleWindow("code-538", "name-236");
        Check.Equal(item.Status, MeritCycleWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleWindowMovesToQueued()
    {
        var item = MeritCycleWindow.NewMeritCycleWindow("code-538", "name-236");
        try
        {
            item.MoveTo(MeritCycleWindowStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleWindowStatus.Queued, "status");
    }

    public static void MeritCycleWindowRejectsMovesFromFailed()
    {
        var item = MeritCycleWindow.NewMeritCycleWindow("code-538", "name-236");
        item.Status = MeritCycleWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(MeritCycleWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        MeritCycleWindowStartsInNew();
        MeritCycleWindowMovesToQueued();
        MeritCycleWindowRejectsMovesFromFailed();
    }
}
