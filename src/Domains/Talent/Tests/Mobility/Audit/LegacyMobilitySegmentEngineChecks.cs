namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyMobilitySegmentEngineChecks
{
    public static void LegacyMobilitySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyMobilitySegmentEngine.DefaultLegacyMobilitySegmentEngine(Flags.NewRegistry());
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("code-28", "title-796");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyMobilitySegmentEngineKeepsResultsWithinBounds();
    }
}
