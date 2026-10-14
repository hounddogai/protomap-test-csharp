namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredPayBandScheduleViewChecks
{
    public static void TieredPayBandScheduleViewShowsTheStatusInCapitals()
    {
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("code-710", "name-550");
        var view = TieredPayBandScheduleView.ToTieredPayBandScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPayBandScheduleView.ToTieredPayBandScheduleViews(new List<TieredPayBandSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPayBandScheduleViewShowsTheStatusInCapitals();
    }
}
