namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityBundleEngineChecks
{
    public static void MobilityBundleEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityBundleEngine.DefaultMobilityBundleEngine();
        var item = MobilityBundle.NewMobilityBundle("code-437", "title-898");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityBundleEngineKeepsResultsWithinBounds();
    }
}
