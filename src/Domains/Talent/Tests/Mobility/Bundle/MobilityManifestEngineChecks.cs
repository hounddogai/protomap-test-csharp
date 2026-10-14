namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityManifestEngineChecks
{
    public static void MobilityManifestEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityManifestEngine.DefaultMobilityManifestEngine();
        var item = MobilityManifest.NewMobilityManifest("code-55", "name-247");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityManifestEngineKeepsResultsWithinBounds();
    }
}
