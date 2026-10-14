namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayAdjustmentViewChecks
{
    public static void HolidayAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = HolidayAdjustment.NewHolidayAdjustment("code-164", "label-458");
        var view = HolidayAdjustmentView.ToHolidayAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayAdjustmentView.ToHolidayAdjustmentViews(new List<HolidayAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayAdjustmentViewShowsTheStatusInCapitals();
    }
}
