namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class PrimaryBonusPoolTierChecks
{
    public static void PrimaryBonusPoolTierStartsInCreated()
    {
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("code-572", "title-366");
        Check.Equal(item.Status, PrimaryBonusPoolTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryBonusPoolTierMovesToPublished()
    {
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("code-572", "title-366");
        try
        {
            item.MoveTo(PrimaryBonusPoolTierStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryBonusPoolTierStatus.Published, "status");
    }

    public static void PrimaryBonusPoolTierRejectsMovesFromRetired()
    {
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("code-572", "title-366");
        item.Status = PrimaryBonusPoolTierStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryBonusPoolTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryBonusPoolTierStartsInCreated();
        PrimaryBonusPoolTierMovesToPublished();
        PrimaryBonusPoolTierRejectsMovesFromRetired();
    }
}
