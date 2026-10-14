namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingMeritCycleSegmentEngineChecks
{
    public static void RollingMeritCycleSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = RollingMeritCycleSegmentEngine.DefaultRollingMeritCycleSegmentEngine(Flags.NewRegistry());
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("code-392", "label-236");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingMeritCycleSegmentEngineKeepsResultsWithinBounds();
    }
}
