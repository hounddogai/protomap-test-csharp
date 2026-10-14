namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class WeightedBonusPoolMilestoneChecks
{
    public static void WeightedBonusPoolMilestoneStartsInPlanned()
    {
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("code-24", "label-525");
        Check.Equal(item.Status, WeightedBonusPoolMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBonusPoolMilestoneMovesToScheduled()
    {
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("code-24", "label-525");
        try
        {
            item.MoveTo(WeightedBonusPoolMilestoneStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBonusPoolMilestoneStatus.Scheduled, "status");
    }

    public static void WeightedBonusPoolMilestoneRejectsMovesFromCancelled()
    {
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("code-24", "label-525");
        item.Status = WeightedBonusPoolMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedBonusPoolMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedBonusPoolMilestoneStartsInPlanned();
        WeightedBonusPoolMilestoneMovesToScheduled();
        WeightedBonusPoolMilestoneRejectsMovesFromCancelled();
    }
}
