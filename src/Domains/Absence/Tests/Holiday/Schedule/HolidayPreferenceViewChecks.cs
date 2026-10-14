namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HolidayPreferenceViewChecks
{
    public static void HolidayPreferenceViewShowsTheStatusInCapitals()
    {
        var item = HolidayPreference.NewHolidayPreference("code-179", "title-514");
        var view = HolidayPreferenceView.ToHolidayPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayPreferenceView.ToHolidayPreferenceViews(new List<HolidayPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayPreferenceViewShowsTheStatusInCapitals();
    }
}
