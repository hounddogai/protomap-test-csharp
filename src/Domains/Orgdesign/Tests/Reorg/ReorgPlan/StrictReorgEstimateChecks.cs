namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class StrictReorgEstimateChecks
{
    public static void StrictReorgEstimateStartsInIdle()
    {
        var item = StrictReorgEstimate.NewStrictReorgEstimate("code-28", "name-85");
        Check.Equal(item.Status, StrictReorgEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReorgEstimateMovesToWarming()
    {
        var item = StrictReorgEstimate.NewStrictReorgEstimate("code-28", "name-85");
        try
        {
            item.MoveTo(StrictReorgEstimateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReorgEstimateStatus.Warming, "status");
    }

    public static void StrictReorgEstimateRejectsMovesFromStopped()
    {
        var item = StrictReorgEstimate.NewStrictReorgEstimate("code-28", "name-85");
        item.Status = StrictReorgEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictReorgEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictReorgEstimateStartsInIdle();
        StrictReorgEstimateMovesToWarming();
        StrictReorgEstimateRejectsMovesFromStopped();
    }
}
