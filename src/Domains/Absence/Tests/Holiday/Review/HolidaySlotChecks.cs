namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidaySlotChecks
{
    public static void HolidaySlotStartsInRequested()
    {
        var item = HolidaySlot.NewHolidaySlot("code-354", "title-424");
        Check.Equal(item.Status, HolidaySlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidaySlotMovesToGranted()
    {
        var item = HolidaySlot.NewHolidaySlot("code-354", "title-424");
        try
        {
            item.MoveTo(HolidaySlotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidaySlotStatus.Granted, "status");
    }

    public static void HolidaySlotRejectsMovesFromExpired()
    {
        var item = HolidaySlot.NewHolidaySlot("code-354", "title-424");
        item.Status = HolidaySlotStatus.Expired;
        Check.Fails(() => item.MoveTo(HolidaySlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HolidaySlotStartsInRequested();
        HolidaySlotMovesToGranted();
        HolidaySlotRejectsMovesFromExpired();
    }
}
