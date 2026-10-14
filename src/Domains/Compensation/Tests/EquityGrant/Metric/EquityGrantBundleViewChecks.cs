namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantBundleViewChecks
{
    public static void EquityGrantBundleViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantBundle.NewEquityGrantBundle("code-907", "label-633");
        var view = EquityGrantBundleView.ToEquityGrantBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantBundleView.ToEquityGrantBundleViews(new List<EquityGrantBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantBundleViewShowsTheStatusInCapitals();
    }
}
