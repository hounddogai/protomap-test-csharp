namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolTierChecks
{
    public static void BonusPoolTierStartsInPlanned()
    {
        var item = BonusPoolTier.NewBonusPoolTier("code-715", "label-938");
        Check.Equal(item.Status, BonusPoolTierStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolTierMovesToCompleted()
    {
        var item = BonusPoolTier.NewBonusPoolTier("code-715", "label-938");
        try
        {
            item.MoveTo(BonusPoolTierStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolTierStatus.Completed, "status");
    }

    public static void BonusPoolTierRejectsMovesFromCancelled()
    {
        var item = BonusPoolTier.NewBonusPoolTier("code-715", "label-938");
        item.Status = BonusPoolTierStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BonusPoolTierStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BonusPoolTierStartsInPlanned();
        BonusPoolTierMovesToCompleted();
        BonusPoolTierRejectsMovesFromCancelled();
    }
}
