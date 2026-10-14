namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictBonusPoolVarianceChecks
{
    public static void StrictBonusPoolVarianceStartsInProposed()
    {
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("code-411", "label-108");
        Check.Equal(item.Status, StrictBonusPoolVarianceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictBonusPoolVarianceMovesToApproved()
    {
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("code-411", "label-108");
        try
        {
            item.MoveTo(StrictBonusPoolVarianceStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictBonusPoolVarianceStatus.Approved, "status");
    }

    public static void StrictBonusPoolVarianceRejectsMovesFromWithdrawn()
    {
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("code-411", "label-108");
        item.Status = StrictBonusPoolVarianceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictBonusPoolVarianceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictBonusPoolVarianceStartsInProposed();
        StrictBonusPoolVarianceMovesToApproved();
        StrictBonusPoolVarianceRejectsMovesFromWithdrawn();
    }
}
