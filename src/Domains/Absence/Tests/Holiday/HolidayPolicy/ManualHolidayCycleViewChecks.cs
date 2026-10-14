namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class ManualHolidayCycleViewChecks
{
    public static void ManualHolidayCycleViewShowsTheStatusInCapitals()
    {
        var item = ManualHolidayCycle.NewManualHolidayCycle("code-972", "label-934");
        var view = ManualHolidayCycleView.ToManualHolidayCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualHolidayCycleView.ToManualHolidayCycleViews(new List<ManualHolidayCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualHolidayCycleViewShowsTheStatusInCapitals();
    }
}
