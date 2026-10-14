namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleEstimateChecks
{
    public static void MeritCycleEstimateStartsInProposed()
    {
        var item = MeritCycleEstimate.NewMeritCycleEstimate("code-789", "label-453");
        Check.Equal(item.Status, MeritCycleEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleEstimateMovesToRejected()
    {
        var item = MeritCycleEstimate.NewMeritCycleEstimate("code-789", "label-453");
        try
        {
            item.MoveTo(MeritCycleEstimateStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleEstimateStatus.Rejected, "status");
    }

    public static void MeritCycleEstimateRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleEstimate.NewMeritCycleEstimate("code-789", "label-453");
        item.Status = MeritCycleEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleEstimateStartsInProposed();
        MeritCycleEstimateMovesToRejected();
        MeritCycleEstimateRejectsMovesFromWithdrawn();
    }
}
