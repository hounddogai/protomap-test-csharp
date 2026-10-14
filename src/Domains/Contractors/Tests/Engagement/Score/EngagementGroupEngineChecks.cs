namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementGroupEngineChecks
{
    public static void EngagementGroupEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementGroupEngine.DefaultEngagementGroupEngine();
        var item = EngagementGroup.NewEngagementGroup("code-572", "label-646");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementGroupEngineKeepsResultsWithinBounds();
    }
}
