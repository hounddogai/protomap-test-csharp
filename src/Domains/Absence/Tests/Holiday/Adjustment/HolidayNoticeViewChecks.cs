namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayNoticeViewChecks
{
    public static void HolidayNoticeViewShowsTheStatusInCapitals()
    {
        var item = HolidayNotice.NewHolidayNotice("code-663", "name-387");
        var view = HolidayNoticeView.ToHolidayNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayNoticeView.ToHolidayNoticeViews(new List<HolidayNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayNoticeViewShowsTheStatusInCapitals();
    }
}
