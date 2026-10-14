namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayBandPolicyViewChecks
{
    public static void PayBandPolicyViewShowsTheStatusInCapitals()
    {
        var item = PayBandPolicy.NewPayBandPolicy("code-608", "title-306");
        var view = PayBandPolicyView.ToPayBandPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandPolicyView.ToPayBandPolicyViews(new List<PayBandPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandPolicyViewShowsTheStatusInCapitals();
    }
}
