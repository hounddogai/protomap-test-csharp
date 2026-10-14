namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolBaselineChecks
{
    public static void BonusPoolBaselineStartsInProposed()
    {
        var item = BonusPoolBaseline.NewBonusPoolBaseline("code-57", "title-23");
        Check.Equal(item.Status, BonusPoolBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolBaselineMovesToApproved()
    {
        var item = BonusPoolBaseline.NewBonusPoolBaseline("code-57", "title-23");
        try
        {
            item.MoveTo(BonusPoolBaselineStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolBaselineStatus.Approved, "status");
    }

    public static void BonusPoolBaselineRejectsMovesFromWithdrawn()
    {
        var item = BonusPoolBaseline.NewBonusPoolBaseline("code-57", "title-23");
        item.Status = BonusPoolBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BonusPoolBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BonusPoolBaselineStartsInProposed();
        BonusPoolBaselineMovesToApproved();
        BonusPoolBaselineRejectsMovesFromWithdrawn();
    }
}
