namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class SeasonalHolidayAllocationViewChecks
{
    public static void SeasonalHolidayAllocationViewShowsTheStatusInCapitals()
    {
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("code-541", "name-720");
        var view = SeasonalHolidayAllocationView.ToSeasonalHolidayAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalHolidayAllocationView.ToSeasonalHolidayAllocationViews(new List<SeasonalHolidayAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalHolidayAllocationViewShowsTheStatusInCapitals();
    }
}
