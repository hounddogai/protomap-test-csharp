namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BatchedMobilityManifestViewChecks
{
    public static void BatchedMobilityManifestViewShowsTheStatusInCapitals()
    {
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("code-370", "label-33");
        var view = BatchedMobilityManifestView.ToBatchedMobilityManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedMobilityManifestView.ToBatchedMobilityManifestViews(new List<BatchedMobilityManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedMobilityManifestViewShowsTheStatusInCapitals();
    }
}
