namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class WeightedReorgPlanChecks
{
    public static void WeightedReorgPlanStartsInRequested()
    {
        var item = WeightedReorgPlan.NewWeightedReorgPlan("code-400", "title-443");
        Check.Equal(item.Status, WeightedReorgPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedReorgPlanMovesToGranted()
    {
        var item = WeightedReorgPlan.NewWeightedReorgPlan("code-400", "title-443");
        try
        {
            item.MoveTo(WeightedReorgPlanStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedReorgPlanStatus.Granted, "status");
    }

    public static void WeightedReorgPlanRejectsMovesFromExpired()
    {
        var item = WeightedReorgPlan.NewWeightedReorgPlan("code-400", "title-443");
        item.Status = WeightedReorgPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedReorgPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedReorgPlanStartsInRequested();
        WeightedReorgPlanMovesToGranted();
        WeightedReorgPlanRejectsMovesFromExpired();
    }
}
