namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBonusPoolSlotChecks
{
    public static void LenientBonusPoolSlotStartsInCreated()
    {
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("code-203", "title-221");
        Check.Equal(item.Status, LenientBonusPoolSlotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBonusPoolSlotMovesToRetired()
    {
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("code-203", "title-221");
        try
        {
            item.MoveTo(LenientBonusPoolSlotStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBonusPoolSlotStatus.Retired, "status");
    }

    public static void LenientBonusPoolSlotRejectsMovesFromRetired()
    {
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("code-203", "title-221");
        item.Status = LenientBonusPoolSlotStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientBonusPoolSlotStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientBonusPoolSlotStartsInCreated();
        LenientBonusPoolSlotMovesToRetired();
        LenientBonusPoolSlotRejectsMovesFromRetired();
    }
}
