namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualSlotChecks
{
    public static void AccrualSlotStartsInRequested()
    {
        var item = AccrualSlot.NewAccrualSlot("code-110", "name-275");
        Check.Equal(item.Status, AccrualSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualSlotMovesToGranted()
    {
        var item = AccrualSlot.NewAccrualSlot("code-110", "name-275");
        try
        {
            item.MoveTo(AccrualSlotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualSlotStatus.Granted, "status");
    }

    public static void AccrualSlotRejectsMovesFromExpired()
    {
        var item = AccrualSlot.NewAccrualSlot("code-110", "name-275");
        item.Status = AccrualSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(AccrualSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccrualSlotStartsInRequested();
        AccrualSlotMovesToGranted();
        AccrualSlotRejectsMovesFromExpired();
    }
}
