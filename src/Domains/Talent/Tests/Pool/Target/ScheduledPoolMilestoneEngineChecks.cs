namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledPoolMilestoneEngineChecks
{
    public static void ScheduledPoolMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPoolMilestoneEngine.DefaultScheduledPoolMilestoneEngine(Flags.NewRegistry());
        var item = ScheduledPoolMilestone.NewScheduledPoolMilestone("code-989", "label-204");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPoolMilestoneEngineKeepsResultsWithinBounds();
    }
}
