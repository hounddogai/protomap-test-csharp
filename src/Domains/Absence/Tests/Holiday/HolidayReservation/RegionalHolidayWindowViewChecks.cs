namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class RegionalHolidayWindowViewChecks
{
    public static void RegionalHolidayWindowViewShowsTheStatusInCapitals()
    {
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("code-705", "label-811");
        var view = RegionalHolidayWindowView.ToRegionalHolidayWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalHolidayWindowView.ToRegionalHolidayWindowViews(new List<RegionalHolidayWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalHolidayWindowViewShowsTheStatusInCapitals();
    }
}
