namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveCostModelCheckpointViewChecks
{
    public static void AdaptiveCostModelCheckpointViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("code-80", "name-322");
        var view = AdaptiveCostModelCheckpointView.ToAdaptiveCostModelCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveCostModelCheckpointView.ToAdaptiveCostModelCheckpointViews(new List<AdaptiveCostModelCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveCostModelCheckpointViewShowsTheStatusInCapitals();
    }
}
