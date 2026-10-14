namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LenientHolidayAdjustmentViewChecks
{
    public static void LenientHolidayAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("code-210", "name-107");
        var view = LenientHolidayAdjustmentView.ToLenientHolidayAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientHolidayAdjustmentView.ToLenientHolidayAdjustmentViews(new List<LenientHolidayAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientHolidayAdjustmentViewShowsTheStatusInCapitals();
    }
}
