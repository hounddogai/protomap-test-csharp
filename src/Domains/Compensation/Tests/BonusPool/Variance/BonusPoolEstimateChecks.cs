namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BonusPoolEstimateChecks
{
    public static void BonusPoolEstimateStartsInProposed()
    {
        var item = BonusPoolEstimate.NewBonusPoolEstimate("code-378", "name-312");
        Check.Equal(item.Status, BonusPoolEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolEstimateMovesToApproved()
    {
        var item = BonusPoolEstimate.NewBonusPoolEstimate("code-378", "name-312");
        try
        {
            item.MoveTo(BonusPoolEstimateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolEstimateStatus.Approved, "status");
    }

    public static void BonusPoolEstimateRejectsMovesFromWithdrawn()
    {
        var item = BonusPoolEstimate.NewBonusPoolEstimate("code-378", "name-312");
        item.Status = BonusPoolEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BonusPoolEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BonusPoolEstimateStartsInProposed();
        BonusPoolEstimateMovesToApproved();
        BonusPoolEstimateRejectsMovesFromWithdrawn();
    }
}
