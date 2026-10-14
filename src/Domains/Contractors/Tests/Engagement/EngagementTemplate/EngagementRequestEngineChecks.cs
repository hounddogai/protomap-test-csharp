namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class EngagementRequestEngineChecks
{
    public static void EngagementRequestEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementRequestEngine.DefaultEngagementRequestEngine(Flags.NewRegistry());
        var item = EngagementRequest.NewEngagementRequest("code-732", "label-855");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementRequestEngineKeepsResultsWithinBounds();
    }
}
