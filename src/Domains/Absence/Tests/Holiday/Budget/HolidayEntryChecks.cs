namespace Acme.Workforce.Absence.Tests.Holiday.Budget;

using Acme.Workforce.Absence.Holiday.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HolidayEntryChecks
{
    public static void HolidayEntryStartsInOpen()
    {
        var item = HolidayEntry.NewHolidayEntry("code-966", "title-532");
        Check.Equal(item.Status, HolidayEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayEntryMovesToReopened()
    {
        var item = HolidayEntry.NewHolidayEntry("code-966", "title-532");
        try
        {
            item.MoveTo(HolidayEntryStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayEntryStatus.Reopened, "status");
    }

    public static void HolidayEntryRejectsMovesFromArchived()
    {
        var item = HolidayEntry.NewHolidayEntry("code-966", "title-532");
        item.Status = HolidayEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(HolidayEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        HolidayEntryStartsInOpen();
        HolidayEntryMovesToReopened();
        HolidayEntryRejectsMovesFromArchived();
    }
}
