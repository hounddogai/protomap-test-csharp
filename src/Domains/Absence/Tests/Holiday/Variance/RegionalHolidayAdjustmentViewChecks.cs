namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalHolidayAdjustmentViewChecks
{
    public static void RegionalHolidayAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("code-458", "title-330");
        var view = RegionalHolidayAdjustmentView.ToRegionalHolidayAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalHolidayAdjustmentView.ToRegionalHolidayAdjustmentViews(new List<RegionalHolidayAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalHolidayAdjustmentViewShowsTheStatusInCapitals();
    }
}
