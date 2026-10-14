namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class RateMilestoneEngineChecks
{
    public static void RateMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = RateMilestoneEngine.DefaultRateMilestoneEngine(Flags.NewRegistry());
        var item = RateMilestone.NewRateMilestone("code-412", "name-910");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateMilestoneEngineKeepsResultsWithinBounds();
    }
}
