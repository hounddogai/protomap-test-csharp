namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class CappedPayBandThresholdViewChecks
{
    public static void CappedPayBandThresholdViewShowsTheStatusInCapitals()
    {
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("code-343", "label-105");
        var view = CappedPayBandThresholdView.ToCappedPayBandThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPayBandThresholdView.ToCappedPayBandThresholdViews(new List<CappedPayBandThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPayBandThresholdViewShowsTheStatusInCapitals();
    }
}
