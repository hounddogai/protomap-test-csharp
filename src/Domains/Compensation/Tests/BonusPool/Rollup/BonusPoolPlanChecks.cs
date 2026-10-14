namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolPlanChecks
{
    public static void BonusPoolPlanStartsInOpen()
    {
        var item = BonusPoolPlan.NewBonusPoolPlan("code-46", "name-929");
        Check.Equal(item.Status, BonusPoolPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolPlanMovesToResolved()
    {
        var item = BonusPoolPlan.NewBonusPoolPlan("code-46", "name-929");
        try
        {
            item.MoveTo(BonusPoolPlanStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolPlanStatus.Resolved, "status");
    }

    public static void BonusPoolPlanRejectsMovesFromArchived()
    {
        var item = BonusPoolPlan.NewBonusPoolPlan("code-46", "name-929");
        item.Status = BonusPoolPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(BonusPoolPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        BonusPoolPlanStartsInOpen();
        BonusPoolPlanMovesToResolved();
        BonusPoolPlanRejectsMovesFromArchived();
    }
}
