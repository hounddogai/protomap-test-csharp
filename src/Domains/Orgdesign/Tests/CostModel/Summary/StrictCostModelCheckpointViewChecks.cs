namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictCostModelCheckpointViewChecks
{
    public static void StrictCostModelCheckpointViewShowsTheStatusInCapitals()
    {
        var item = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint("code-760", "label-801");
        var view = StrictCostModelCheckpointView.ToStrictCostModelCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictCostModelCheckpointView.ToStrictCostModelCheckpointViews(new List<StrictCostModelCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictCostModelCheckpointViewShowsTheStatusInCapitals();
    }
}
