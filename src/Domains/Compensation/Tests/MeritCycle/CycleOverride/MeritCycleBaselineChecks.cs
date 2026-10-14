namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleBaselineChecks
{
    public static void MeritCycleBaselineStartsInProposed()
    {
        var item = MeritCycleBaseline.NewMeritCycleBaseline("code-972", "name-320");
        Check.Equal(item.Status, MeritCycleBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleBaselineMovesToRejected()
    {
        var item = MeritCycleBaseline.NewMeritCycleBaseline("code-972", "name-320");
        try
        {
            item.MoveTo(MeritCycleBaselineStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleBaselineStatus.Rejected, "status");
    }

    public static void MeritCycleBaselineRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleBaseline.NewMeritCycleBaseline("code-972", "name-320");
        item.Status = MeritCycleBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleBaselineStartsInProposed();
        MeritCycleBaselineMovesToRejected();
        MeritCycleBaselineRejectsMovesFromWithdrawn();
    }
}
