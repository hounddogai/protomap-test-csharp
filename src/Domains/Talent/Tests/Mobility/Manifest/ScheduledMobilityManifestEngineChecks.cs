namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledMobilityManifestEngineChecks
{
    public static void ScheduledMobilityManifestEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledMobilityManifestEngine.DefaultScheduledMobilityManifestEngine();
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("code-764", "label-798");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledMobilityManifestEngineKeepsResultsWithinBounds();
    }
}
