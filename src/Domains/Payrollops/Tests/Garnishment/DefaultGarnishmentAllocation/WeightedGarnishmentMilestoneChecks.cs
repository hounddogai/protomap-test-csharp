namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class WeightedGarnishmentMilestoneChecks
{
    public static void WeightedGarnishmentMilestoneStartsInNew()
    {
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("code-474", "title-665");
        Check.Equal(item.Status, WeightedGarnishmentMilestoneStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedGarnishmentMilestoneMovesToFailed()
    {
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("code-474", "title-665");
        try
        {
            item.MoveTo(WeightedGarnishmentMilestoneStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedGarnishmentMilestoneStatus.Failed, "status");
    }

    public static void WeightedGarnishmentMilestoneRejectsMovesFromFailed()
    {
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("code-474", "title-665");
        item.Status = WeightedGarnishmentMilestoneStatus.Failed;
        Check.Fails(() => item.MoveTo(WeightedGarnishmentMilestoneStatus.New), "move");
    }

    public static void RunAll()
    {
        WeightedGarnishmentMilestoneStartsInNew();
        WeightedGarnishmentMilestoneMovesToFailed();
        WeightedGarnishmentMilestoneRejectsMovesFromFailed();
    }
}
