namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleSegmentEngineChecks
{
    public static void MeritCycleSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleSegmentEngine.DefaultMeritCycleSegmentEngine();
        var item = MeritCycleSegment.NewMeritCycleSegment("code-129", "title-371");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleSegmentEngineKeepsResultsWithinBounds();
    }
}
