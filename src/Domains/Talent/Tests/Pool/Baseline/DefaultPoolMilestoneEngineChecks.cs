namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolMilestoneEngineChecks
{
    public static void DefaultPoolMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPoolMilestoneEngine.DefaultDefaultPoolMilestoneEngine(Flags.NewRegistry());
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("code-919", "name-870");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPoolMilestoneEngineKeepsResultsWithinBounds();
    }
}
