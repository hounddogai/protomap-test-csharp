namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandBaselineViewChecks
{
    public static void PayBandBaselineViewShowsTheStatusInCapitals()
    {
        var item = PayBandBaseline.NewPayBandBaseline("code-342", "title-257");
        var view = PayBandBaselineView.ToPayBandBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandBaselineView.ToPayBandBaselineViews(new List<PayBandBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandBaselineViewShowsTheStatusInCapitals();
    }
}
