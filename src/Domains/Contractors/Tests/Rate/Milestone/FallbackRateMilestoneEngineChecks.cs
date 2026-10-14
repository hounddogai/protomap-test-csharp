namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class FallbackRateMilestoneEngineChecks
{
    public static void FallbackRateMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackRateMilestoneEngine.DefaultFallbackRateMilestoneEngine();
        var item = FallbackRateMilestone.NewFallbackRateMilestone("code-443", "name-30");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackRateMilestoneEngineKeepsResultsWithinBounds();
    }
}
