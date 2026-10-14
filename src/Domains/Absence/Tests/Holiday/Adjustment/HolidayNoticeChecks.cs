namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayNoticeChecks
{
    public static void HolidayNoticeStartsInNew()
    {
        var item = HolidayNotice.NewHolidayNotice("code-663", "name-387");
        Check.Equal(item.Status, HolidayNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayNoticeMovesToDone()
    {
        var item = HolidayNotice.NewHolidayNotice("code-663", "name-387");
        try
        {
            item.MoveTo(HolidayNoticeStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayNoticeStatus.Done, "status");
    }

    public static void HolidayNoticeRejectsMovesFromFailed()
    {
        var item = HolidayNotice.NewHolidayNotice("code-663", "name-387");
        item.Status = HolidayNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(HolidayNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        HolidayNoticeStartsInNew();
        HolidayNoticeMovesToDone();
        HolidayNoticeRejectsMovesFromFailed();
    }
}
