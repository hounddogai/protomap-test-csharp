namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PrimaryBalanceMetricViewChecks
{
    public static void PrimaryBalanceMetricViewShowsTheStatusInCapitals()
    {
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("code-58", "title-534");
        var view = PrimaryBalanceMetricView.ToPrimaryBalanceMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryBalanceMetricView.ToPrimaryBalanceMetricViews(new List<PrimaryBalanceMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryBalanceMetricViewShowsTheStatusInCapitals();
    }
}
