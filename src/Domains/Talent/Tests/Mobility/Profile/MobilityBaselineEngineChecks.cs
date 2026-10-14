namespace Acme.Workforce.Talent.Tests.Mobility.Profile;

using Acme.Workforce.Talent.Mobility.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MobilityBaselineEngineChecks
{
    public static void MobilityBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityBaselineEngine.DefaultMobilityBaselineEngine();
        var item = MobilityBaseline.NewMobilityBaseline("code-16", "title-9");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityBaselineEngineKeepsResultsWithinBounds();
    }
}
