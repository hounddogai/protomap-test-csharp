namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class LegacyBonusPoolAllocationChecks
{
    public static void LegacyBonusPoolAllocationStartsInNew()
    {
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("code-296", "title-345");
        Check.Equal(item.Status, LegacyBonusPoolAllocationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyBonusPoolAllocationMovesToQueued()
    {
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("code-296", "title-345");
        try
        {
            item.MoveTo(LegacyBonusPoolAllocationStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyBonusPoolAllocationStatus.Queued, "status");
    }

    public static void LegacyBonusPoolAllocationRejectsMovesFromFailed()
    {
        var item = LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation("code-296", "title-345");
        item.Status = LegacyBonusPoolAllocationStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyBonusPoolAllocationStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyBonusPoolAllocationStartsInNew();
        LegacyBonusPoolAllocationMovesToQueued();
        LegacyBonusPoolAllocationRejectsMovesFromFailed();
    }
}
