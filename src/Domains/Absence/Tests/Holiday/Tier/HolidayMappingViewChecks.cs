namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayMappingViewChecks
{
    public static void HolidayMappingViewShowsTheStatusInCapitals()
    {
        var item = HolidayMapping.NewHolidayMapping("code-726", "label-103");
        var view = HolidayMappingView.ToHolidayMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayMappingView.ToHolidayMappingViews(new List<HolidayMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayMappingViewShowsTheStatusInCapitals();
    }
}
