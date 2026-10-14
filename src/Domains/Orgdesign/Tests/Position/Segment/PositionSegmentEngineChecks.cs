namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PositionSegmentEngineChecks
{
    public static void PositionSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = PositionSegmentEngine.DefaultPositionSegmentEngine(Flags.NewRegistry());
        var item = PositionSegment.NewPositionSegment("code-563", "label-709");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionSegmentEngineKeepsResultsWithinBounds();
    }
}
