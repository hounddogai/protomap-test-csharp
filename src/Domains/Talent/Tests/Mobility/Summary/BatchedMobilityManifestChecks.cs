namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BatchedMobilityManifestChecks
{
    public static void BatchedMobilityManifestStartsInCreated()
    {
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("code-370", "label-33");
        Check.Equal(item.Status, BatchedMobilityManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedMobilityManifestMovesToValidated()
    {
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("code-370", "label-33");
        try
        {
            item.MoveTo(BatchedMobilityManifestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedMobilityManifestStatus.Validated, "status");
    }

    public static void BatchedMobilityManifestRejectsMovesFromRetired()
    {
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("code-370", "label-33");
        item.Status = BatchedMobilityManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedMobilityManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedMobilityManifestStartsInCreated();
        BatchedMobilityManifestMovesToValidated();
        BatchedMobilityManifestRejectsMovesFromRetired();
    }
}
