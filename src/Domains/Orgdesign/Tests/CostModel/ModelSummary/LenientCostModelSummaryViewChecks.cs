namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LenientCostModelSummaryViewChecks
{
    public static void LenientCostModelSummaryViewShowsTheStatusInCapitals()
    {
        var item = LenientCostModelSummary.NewLenientCostModelSummary("code-309", "name-983");
        var view = LenientCostModelSummaryView.ToLenientCostModelSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientCostModelSummaryView.ToLenientCostModelSummaryViews(new List<LenientCostModelSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientCostModelSummaryViewShowsTheStatusInCapitals();
    }
}
