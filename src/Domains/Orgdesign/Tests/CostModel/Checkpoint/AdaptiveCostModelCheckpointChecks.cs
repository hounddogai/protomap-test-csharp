namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveCostModelCheckpointChecks
{
    public static void AdaptiveCostModelCheckpointStartsInNew()
    {
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("code-80", "name-322");
        Check.Equal(item.Status, AdaptiveCostModelCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCostModelCheckpointMovesToQueued()
    {
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("code-80", "name-322");
        try
        {
            item.MoveTo(AdaptiveCostModelCheckpointStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCostModelCheckpointStatus.Queued, "status");
    }

    public static void AdaptiveCostModelCheckpointRejectsMovesFromFailed()
    {
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("code-80", "name-322");
        item.Status = AdaptiveCostModelCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveCostModelCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveCostModelCheckpointStartsInNew();
        AdaptiveCostModelCheckpointMovesToQueued();
        AdaptiveCostModelCheckpointRejectsMovesFromFailed();
    }
}
