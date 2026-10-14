namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantSummaryViewChecks
{
    public static void EquityGrantSummaryViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantSummary.NewEquityGrantSummary("code-766", "label-159");
        var view = EquityGrantSummaryView.ToEquityGrantSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantSummaryView.ToEquityGrantSummaryViews(new List<EquityGrantSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantSummaryViewShowsTheStatusInCapitals();
    }
}
