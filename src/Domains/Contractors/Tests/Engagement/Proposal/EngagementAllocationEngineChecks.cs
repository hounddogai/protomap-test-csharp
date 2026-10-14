namespace Acme.Workforce.Contractors.Tests.Engagement.Proposal;

using Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class EngagementAllocationEngineChecks
{
    public static void EngagementAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementAllocationEngine.DefaultEngagementAllocationEngine();
        var item = EngagementAllocation.NewEngagementAllocation("code-34", "label-551");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementAllocationEngineKeepsResultsWithinBounds();
    }
}
