namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgCheckpointViewChecks
{
    public static void ReorgCheckpointViewShowsTheStatusInCapitals()
    {
        var item = ReorgCheckpoint.NewReorgCheckpoint("code-880", "name-406");
        var view = ReorgCheckpointView.ToReorgCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgCheckpointView.ToReorgCheckpointViews(new List<ReorgCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgCheckpointViewShowsTheStatusInCapitals();
    }
}
