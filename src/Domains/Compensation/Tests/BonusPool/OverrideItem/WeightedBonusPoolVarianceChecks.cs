namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedBonusPoolVarianceChecks
{
    public static void WeightedBonusPoolVarianceStartsInProposed()
    {
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("code-936", "title-193");
        Check.Equal(item.Status, WeightedBonusPoolVarianceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBonusPoolVarianceMovesToRejected()
    {
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("code-936", "title-193");
        try
        {
            item.MoveTo(WeightedBonusPoolVarianceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBonusPoolVarianceStatus.Rejected, "status");
    }

    public static void WeightedBonusPoolVarianceRejectsMovesFromWithdrawn()
    {
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("code-936", "title-193");
        item.Status = WeightedBonusPoolVarianceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedBonusPoolVarianceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedBonusPoolVarianceStartsInProposed();
        WeightedBonusPoolVarianceMovesToRejected();
        WeightedBonusPoolVarianceRejectsMovesFromWithdrawn();
    }
}
