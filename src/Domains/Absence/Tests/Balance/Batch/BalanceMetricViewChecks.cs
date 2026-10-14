namespace Acme.Workforce.Absence.Tests.Balance.Batch;

using Acme.Workforce.Absence.Balance.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BalanceMetricViewChecks
{
    public static void BalanceMetricViewShowsTheStatusInCapitals()
    {
        var item = BalanceMetric.NewBalanceMetric("code-626", "title-693");
        var view = BalanceMetricView.ToBalanceMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceMetricView.ToBalanceMetricViews(new List<BalanceMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceMetricViewShowsTheStatusInCapitals();
    }
}
