namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandScoreViewChecks
{
    public static void PayBandScoreViewShowsTheStatusInCapitals()
    {
        var item = PayBandScore.NewPayBandScore("code-828", "label-850");
        var view = PayBandScoreView.ToPayBandScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandScoreView.ToPayBandScoreViews(new List<PayBandScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandScoreViewShowsTheStatusInCapitals();
    }
}
