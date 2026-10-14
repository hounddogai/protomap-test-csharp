namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class FallbackBonusPoolMilestoneChecks
{
    public static void FallbackBonusPoolMilestoneStartsInOpen()
    {
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("code-231", "name-436");
        Check.Equal(item.Status, FallbackBonusPoolMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBonusPoolMilestoneMovesToResolved()
    {
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("code-231", "name-436");
        try
        {
            item.MoveTo(FallbackBonusPoolMilestoneStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBonusPoolMilestoneStatus.Resolved, "status");
    }

    public static void FallbackBonusPoolMilestoneRejectsMovesFromArchived()
    {
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("code-231", "name-436");
        item.Status = FallbackBonusPoolMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackBonusPoolMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackBonusPoolMilestoneStartsInOpen();
        FallbackBonusPoolMilestoneMovesToResolved();
        FallbackBonusPoolMilestoneRejectsMovesFromArchived();
    }
}
