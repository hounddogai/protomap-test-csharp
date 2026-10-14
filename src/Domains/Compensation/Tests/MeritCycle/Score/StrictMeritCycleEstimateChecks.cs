namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictMeritCycleEstimateChecks
{
    public static void StrictMeritCycleEstimateStartsInRequested()
    {
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("code-577", "label-745");
        Check.Equal(item.Status, StrictMeritCycleEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictMeritCycleEstimateMovesToRevoked()
    {
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("code-577", "label-745");
        try
        {
            item.MoveTo(StrictMeritCycleEstimateStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictMeritCycleEstimateStatus.Revoked, "status");
    }

    public static void StrictMeritCycleEstimateRejectsMovesFromExpired()
    {
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("code-577", "label-745");
        item.Status = StrictMeritCycleEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictMeritCycleEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictMeritCycleEstimateStartsInRequested();
        StrictMeritCycleEstimateMovesToRevoked();
        StrictMeritCycleEstimateRejectsMovesFromExpired();
    }
}
