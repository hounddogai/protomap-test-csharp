namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultPayBandEstimateViewChecks
{
    public static void DefaultPayBandEstimateViewShowsTheStatusInCapitals()
    {
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("code-510", "title-297");
        var view = DefaultPayBandEstimateView.ToDefaultPayBandEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPayBandEstimateView.ToDefaultPayBandEstimateViews(new List<DefaultPayBandEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPayBandEstimateViewShowsTheStatusInCapitals();
    }
}
