namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class LenientHolidayCycleViewChecks
{
    public static void LenientHolidayCycleViewShowsTheStatusInCapitals()
    {
        var item = LenientHolidayCycle.NewLenientHolidayCycle("code-479", "name-598");
        var view = LenientHolidayCycleView.ToLenientHolidayCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientHolidayCycleView.ToLenientHolidayCycleViews(new List<LenientHolidayCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientHolidayCycleViewShowsTheStatusInCapitals();
    }
}
