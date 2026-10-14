namespace Acme.Workforce.Talent.Tests.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedPoolMilestoneEngineChecks
{
    public static void CappedPoolMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPoolMilestoneEngine.DefaultCappedPoolMilestoneEngine();
        var item = CappedPoolMilestone.NewCappedPoolMilestone("code-43", "name-364");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPoolMilestoneEngineKeepsResultsWithinBounds();
    }
}
