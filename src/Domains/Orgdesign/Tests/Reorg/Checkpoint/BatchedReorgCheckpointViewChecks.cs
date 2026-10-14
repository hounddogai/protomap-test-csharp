namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedReorgCheckpointViewChecks
{
    public static void BatchedReorgCheckpointViewShowsTheStatusInCapitals()
    {
        var item = BatchedReorgCheckpoint.NewBatchedReorgCheckpoint("code-172", "title-821");
        var view = BatchedReorgCheckpointView.ToBatchedReorgCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedReorgCheckpointView.ToBatchedReorgCheckpointViews(new List<BatchedReorgCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedReorgCheckpointViewShowsTheStatusInCapitals();
    }
}
