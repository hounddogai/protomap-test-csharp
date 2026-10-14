namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayBandMappingViewChecks
{
    public static void PayBandMappingViewShowsTheStatusInCapitals()
    {
        var item = PayBandMapping.NewPayBandMapping("code-381", "label-10");
        var view = PayBandMappingView.ToPayBandMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandMappingView.ToPayBandMappingViews(new List<PayBandMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandMappingViewShowsTheStatusInCapitals();
    }
}
