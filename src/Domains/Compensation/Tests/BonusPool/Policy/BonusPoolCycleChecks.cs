namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolCycleChecks
{
    public static void BonusPoolCycleStartsInRequested()
    {
        var item = BonusPoolCycle.NewBonusPoolCycle("code-989", "label-238");
        Check.Equal(item.Status, BonusPoolCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolCycleMovesToRevoked()
    {
        var item = BonusPoolCycle.NewBonusPoolCycle("code-989", "label-238");
        try
        {
            item.MoveTo(BonusPoolCycleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolCycleStatus.Revoked, "status");
    }

    public static void BonusPoolCycleRejectsMovesFromExpired()
    {
        var item = BonusPoolCycle.NewBonusPoolCycle("code-989", "label-238");
        item.Status = BonusPoolCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(BonusPoolCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BonusPoolCycleStartsInRequested();
        BonusPoolCycleMovesToRevoked();
        BonusPoolCycleRejectsMovesFromExpired();
    }
}
