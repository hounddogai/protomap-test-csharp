namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedRateMilestoneEngineChecks
{
    public static void CappedRateMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = CappedRateMilestoneEngine.DefaultCappedRateMilestoneEngine();
        var item = CappedRateMilestone.NewCappedRateMilestone("code-428", "label-654");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedRateMilestoneEngineKeepsResultsWithinBounds();
    }
}
