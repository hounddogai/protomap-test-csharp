namespace Acme.Workforce.Compensation.Tests.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class BonusPoolSlotChecks
{
    public static void BonusPoolSlotStartsInCreated()
    {
        var item = BonusPoolSlot.NewBonusPoolSlot("code-881", "name-222");
        Check.Equal(item.Status, BonusPoolSlotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolSlotMovesToValidated()
    {
        var item = BonusPoolSlot.NewBonusPoolSlot("code-881", "name-222");
        try
        {
            item.MoveTo(BonusPoolSlotStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolSlotStatus.Validated, "status");
    }

    public static void BonusPoolSlotRejectsMovesFromRetired()
    {
        var item = BonusPoolSlot.NewBonusPoolSlot("code-881", "name-222");
        item.Status = BonusPoolSlotStatus.Retired;
        Check.Fails(() => item.MoveTo(BonusPoolSlotStatus.Created), "move");
    }

    public static void RunAll()
    {
        BonusPoolSlotStartsInCreated();
        BonusPoolSlotMovesToValidated();
        BonusPoolSlotRejectsMovesFromRetired();
    }
}
