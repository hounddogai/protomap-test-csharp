namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveHolidayVarianceViewChecks
{
    public static void AdaptiveHolidayVarianceViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("code-800", "label-489");
        var view = AdaptiveHolidayVarianceView.ToAdaptiveHolidayVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveHolidayVarianceView.ToAdaptiveHolidayVarianceViews(new List<AdaptiveHolidayVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveHolidayVarianceViewShowsTheStatusInCapitals();
    }
}
