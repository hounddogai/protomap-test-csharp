namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleProfileChecks
{
    public static void MeritCycleProfileStartsInIdle()
    {
        var item = MeritCycleProfile.NewMeritCycleProfile("code-538", "title-918");
        Check.Equal(item.Status, MeritCycleProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleProfileMovesToDraining()
    {
        var item = MeritCycleProfile.NewMeritCycleProfile("code-538", "title-918");
        try
        {
            item.MoveTo(MeritCycleProfileStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleProfileStatus.Draining, "status");
    }

    public static void MeritCycleProfileRejectsMovesFromStopped()
    {
        var item = MeritCycleProfile.NewMeritCycleProfile("code-538", "title-918");
        item.Status = MeritCycleProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(MeritCycleProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        MeritCycleProfileStartsInIdle();
        MeritCycleProfileMovesToDraining();
        MeritCycleProfileRejectsMovesFromStopped();
    }
}
