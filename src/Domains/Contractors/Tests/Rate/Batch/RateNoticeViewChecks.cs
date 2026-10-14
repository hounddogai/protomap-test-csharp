namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateNoticeViewChecks
{
    public static void RateNoticeViewShowsTheStatusInCapitals()
    {
        var item = RateNotice.NewRateNotice("code-383", "label-716");
        var view = RateNoticeView.ToRateNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateNoticeView.ToRateNoticeViews(new List<RateNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateNoticeViewShowsTheStatusInCapitals();
    }
}
