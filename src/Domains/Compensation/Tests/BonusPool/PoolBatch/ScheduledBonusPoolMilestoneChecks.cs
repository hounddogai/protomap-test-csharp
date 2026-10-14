namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class ScheduledBonusPoolMilestoneChecks
{
    public static void ScheduledBonusPoolMilestoneStartsInOpen()
    {
        var item = ScheduledBonusPoolMilestone.NewScheduledBonusPoolMilestone("code-314", "name-205");
        Check.Equal(item.Status, ScheduledBonusPoolMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBonusPoolMilestoneMovesToInReview()
    {
        var item = ScheduledBonusPoolMilestone.NewScheduledBonusPoolMilestone("code-314", "name-205");
        try
        {
            item.MoveTo(ScheduledBonusPoolMilestoneStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBonusPoolMilestoneStatus.InReview, "status");
    }

    public static void ScheduledBonusPoolMilestoneRejectsMovesFromArchived()
    {
        var item = ScheduledBonusPoolMilestone.NewScheduledBonusPoolMilestone("code-314", "name-205");
        item.Status = ScheduledBonusPoolMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledBonusPoolMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledBonusPoolMilestoneStartsInOpen();
        ScheduledBonusPoolMilestoneMovesToInReview();
        ScheduledBonusPoolMilestoneRejectsMovesFromArchived();
    }
}
