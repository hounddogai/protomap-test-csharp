namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class HolidayTargetViewChecks
{
    public static void HolidayTargetViewShowsTheStatusInCapitals()
    {
        var item = HolidayTarget.NewHolidayTarget("code-221", "name-393");
        var view = HolidayTargetView.ToHolidayTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayTargetView.ToHolidayTargetViews(new List<HolidayTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayTargetViewShowsTheStatusInCapitals();
    }
}
