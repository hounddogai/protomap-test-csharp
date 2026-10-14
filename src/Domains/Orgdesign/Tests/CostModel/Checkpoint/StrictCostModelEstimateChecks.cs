namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictCostModelEstimateChecks
{
    public static void StrictCostModelEstimateStartsInIdle()
    {
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("code-480", "title-5");
        Check.Equal(item.Status, StrictCostModelEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictCostModelEstimateMovesToWarming()
    {
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("code-480", "title-5");
        try
        {
            item.MoveTo(StrictCostModelEstimateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictCostModelEstimateStatus.Warming, "status");
    }

    public static void StrictCostModelEstimateRejectsMovesFromStopped()
    {
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("code-480", "title-5");
        item.Status = StrictCostModelEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictCostModelEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictCostModelEstimateStartsInIdle();
        StrictCostModelEstimateMovesToWarming();
        StrictCostModelEstimateRejectsMovesFromStopped();
    }
}
