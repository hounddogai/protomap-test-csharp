namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolMilestoneChecks
{
    public static void BonusPoolMilestoneStartsInOpen()
    {
        var item = BonusPoolMilestone.NewBonusPoolMilestone("code-658", "title-294");
        Check.Equal(item.Status, BonusPoolMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolMilestoneMovesToArchived()
    {
        var item = BonusPoolMilestone.NewBonusPoolMilestone("code-658", "title-294");
        try
        {
            item.MoveTo(BonusPoolMilestoneStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolMilestoneStatus.Archived, "status");
    }

    public static void BonusPoolMilestoneRejectsMovesFromArchived()
    {
        var item = BonusPoolMilestone.NewBonusPoolMilestone("code-658", "title-294");
        item.Status = BonusPoolMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(BonusPoolMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        BonusPoolMilestoneStartsInOpen();
        BonusPoolMilestoneMovesToArchived();
        BonusPoolMilestoneRejectsMovesFromArchived();
    }
}
