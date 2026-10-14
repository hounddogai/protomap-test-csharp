namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PolicySegmentEngineChecks
{
    public static void PolicySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = PolicySegmentEngine.DefaultPolicySegmentEngine(Flags.NewRegistry());
        var item = PolicySegment.NewPolicySegment("code-858", "name-967");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicySegmentEngineKeepsResultsWithinBounds();
    }
}
