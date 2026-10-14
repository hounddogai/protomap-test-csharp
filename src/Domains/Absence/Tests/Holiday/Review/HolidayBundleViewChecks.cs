namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidayBundleViewChecks
{
    public static void HolidayBundleViewShowsTheStatusInCapitals()
    {
        var item = HolidayBundle.NewHolidayBundle("code-853", "name-747");
        var view = HolidayBundleView.ToHolidayBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayBundleView.ToHolidayBundleViews(new List<HolidayBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayBundleViewShowsTheStatusInCapitals();
    }
}
