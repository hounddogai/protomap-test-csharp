namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedBonusPoolQuotaChecks
{
    public static void WeightedBonusPoolQuotaStartsInProposed()
    {
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("code-939", "name-579");
        Check.Equal(item.Status, WeightedBonusPoolQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBonusPoolQuotaMovesToApproved()
    {
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("code-939", "name-579");
        try
        {
            item.MoveTo(WeightedBonusPoolQuotaStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBonusPoolQuotaStatus.Approved, "status");
    }

    public static void WeightedBonusPoolQuotaRejectsMovesFromWithdrawn()
    {
        var item = WeightedBonusPoolQuota.NewWeightedBonusPoolQuota("code-939", "name-579");
        item.Status = WeightedBonusPoolQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedBonusPoolQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedBonusPoolQuotaStartsInProposed();
        WeightedBonusPoolQuotaMovesToApproved();
        WeightedBonusPoolQuotaRejectsMovesFromWithdrawn();
    }
}
