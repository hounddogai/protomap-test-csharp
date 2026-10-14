namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalHolidayExceptionViewChecks
{
    public static void SeasonalHolidayExceptionViewShowsTheStatusInCapitals()
    {
        var item = SeasonalHolidayException.NewSeasonalHolidayException("code-528", "name-753");
        var view = SeasonalHolidayExceptionView.ToSeasonalHolidayExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalHolidayExceptionView.ToSeasonalHolidayExceptionViews(new List<SeasonalHolidayException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalHolidayExceptionViewShowsTheStatusInCapitals();
    }
}
