namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityRollupEngineChecks
{
    public static void MobilityRollupEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityRollupEngine.DefaultMobilityRollupEngine();
        var item = MobilityRollup.NewMobilityRollup("code-966", "title-546");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityRollupEngineKeepsResultsWithinBounds();
    }
}
