namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandScheduleViewChecks
{
    public static void PayBandScheduleViewShowsTheStatusInCapitals()
    {
        var item = PayBandSchedule.NewPayBandSchedule("code-40", "label-525");
        var view = PayBandScheduleView.ToPayBandScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandScheduleView.ToPayBandScheduleViews(new List<PayBandSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandScheduleViewShowsTheStatusInCapitals();
    }
}
