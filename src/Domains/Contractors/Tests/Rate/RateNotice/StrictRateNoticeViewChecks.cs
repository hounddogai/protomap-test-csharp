namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class StrictRateNoticeViewChecks
{
    public static void StrictRateNoticeViewShowsTheStatusInCapitals()
    {
        var item = StrictRateNotice.NewStrictRateNotice("code-796", "name-953");
        var view = StrictRateNoticeView.ToStrictRateNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictRateNoticeView.ToStrictRateNoticeViews(new List<StrictRateNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictRateNoticeViewShowsTheStatusInCapitals();
    }
}
