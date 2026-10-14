namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleThresholdChecks
{
    public static void MeritCycleThresholdStartsInProposed()
    {
        var item = MeritCycleThreshold.NewMeritCycleThreshold("code-261", "title-501");
        Check.Equal(item.Status, MeritCycleThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleThresholdMovesToWithdrawn()
    {
        var item = MeritCycleThreshold.NewMeritCycleThreshold("code-261", "title-501");
        try
        {
            item.MoveTo(MeritCycleThresholdStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleThresholdStatus.Withdrawn, "status");
    }

    public static void MeritCycleThresholdRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleThreshold.NewMeritCycleThreshold("code-261", "title-501");
        item.Status = MeritCycleThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleThresholdStartsInProposed();
        MeritCycleThresholdMovesToWithdrawn();
        MeritCycleThresholdRejectsMovesFromWithdrawn();
    }
}
