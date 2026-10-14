namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelSummaryViewChecks
{
    public static void TieredCostModelSummaryViewShowsTheStatusInCapitals()
    {
        var item = TieredCostModelSummary.NewTieredCostModelSummary("code-351", "title-199");
        var view = TieredCostModelSummaryView.ToTieredCostModelSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredCostModelSummaryView.ToTieredCostModelSummaryViews(new List<TieredCostModelSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredCostModelSummaryViewShowsTheStatusInCapitals();
    }
}
