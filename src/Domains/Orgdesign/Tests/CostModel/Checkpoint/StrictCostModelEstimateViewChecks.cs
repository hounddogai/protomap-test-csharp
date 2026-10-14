namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictCostModelEstimateViewChecks
{
    public static void StrictCostModelEstimateViewShowsTheStatusInCapitals()
    {
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("code-480", "title-5");
        var view = StrictCostModelEstimateView.ToStrictCostModelEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictCostModelEstimateView.ToStrictCostModelEstimateViews(new List<StrictCostModelEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictCostModelEstimateViewShowsTheStatusInCapitals();
    }
}
