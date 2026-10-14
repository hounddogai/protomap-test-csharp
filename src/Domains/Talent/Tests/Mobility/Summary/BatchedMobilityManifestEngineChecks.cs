namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BatchedMobilityManifestEngineChecks
{
    public static void BatchedMobilityManifestEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedMobilityManifestEngine.DefaultBatchedMobilityManifestEngine(Flags.NewRegistry());
        var item = BatchedMobilityManifest.NewBatchedMobilityManifest("code-370", "label-33");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedMobilityManifestEngineKeepsResultsWithinBounds();
    }
}
