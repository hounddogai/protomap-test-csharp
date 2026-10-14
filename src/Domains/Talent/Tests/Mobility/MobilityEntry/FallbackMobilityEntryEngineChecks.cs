namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class FallbackMobilityEntryEngineChecks
{
    public static void FallbackMobilityEntryEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackMobilityEntryEngine.DefaultFallbackMobilityEntryEngine();
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("code-87", "title-682");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackMobilityEntryEngineKeepsResultsWithinBounds();
    }
}
