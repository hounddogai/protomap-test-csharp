namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolVarianceChecks
{
    public static void BonusPoolVarianceStartsInRequested()
    {
        var item = BonusPoolVariance.NewBonusPoolVariance("code-859", "label-844");
        Check.Equal(item.Status, BonusPoolVarianceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolVarianceMovesToRevoked()
    {
        var item = BonusPoolVariance.NewBonusPoolVariance("code-859", "label-844");
        try
        {
            item.MoveTo(BonusPoolVarianceStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolVarianceStatus.Revoked, "status");
    }

    public static void BonusPoolVarianceRejectsMovesFromExpired()
    {
        var item = BonusPoolVariance.NewBonusPoolVariance("code-859", "label-844");
        item.Status = BonusPoolVarianceStatus.Expired;
        Check.Fails(() => item.MoveTo(BonusPoolVarianceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BonusPoolVarianceStartsInRequested();
        BonusPoolVarianceMovesToRevoked();
        BonusPoolVarianceRejectsMovesFromExpired();
    }
}
