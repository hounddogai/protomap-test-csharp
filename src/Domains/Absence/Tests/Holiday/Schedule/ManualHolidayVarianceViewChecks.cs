namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualHolidayVarianceViewChecks
{
    public static void ManualHolidayVarianceViewShowsTheStatusInCapitals()
    {
        var item = ManualHolidayVariance.NewManualHolidayVariance("code-884", "name-510");
        var view = ManualHolidayVarianceView.ToManualHolidayVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualHolidayVarianceView.ToManualHolidayVarianceViews(new List<ManualHolidayVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualHolidayVarianceViewShowsTheStatusInCapitals();
    }
}
