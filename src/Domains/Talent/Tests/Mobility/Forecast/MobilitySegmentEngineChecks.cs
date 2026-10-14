namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilitySegmentEngineChecks
{
    public static void MobilitySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = MobilitySegmentEngine.DefaultMobilitySegmentEngine(Flags.NewRegistry());
        var item = MobilitySegment.NewMobilitySegment("code-666", "name-811");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilitySegmentEngineKeepsResultsWithinBounds();
    }
}
