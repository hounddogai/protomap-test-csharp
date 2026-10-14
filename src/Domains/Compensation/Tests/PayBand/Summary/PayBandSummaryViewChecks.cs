namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PayBandSummaryViewChecks
{
    public static void PayBandSummaryViewShowsTheStatusInCapitals()
    {
        var item = PayBandSummary.NewPayBandSummary("code-18", "title-11");
        var view = PayBandSummaryView.ToPayBandSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandSummaryView.ToPayBandSummaryViews(new List<PayBandSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandSummaryViewShowsTheStatusInCapitals();
    }
}
