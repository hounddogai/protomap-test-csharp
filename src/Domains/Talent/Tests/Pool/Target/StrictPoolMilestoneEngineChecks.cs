namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictPoolMilestoneEngineChecks
{
    public static void StrictPoolMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPoolMilestoneEngine.DefaultStrictPoolMilestoneEngine();
        var item = StrictPoolMilestone.NewStrictPoolMilestone("code-516", "label-27");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPoolMilestoneEngineKeepsResultsWithinBounds();
    }
}
