namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveVisitorCheckpointEngineChecks
{
    public static void AdaptiveVisitorCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveVisitorCheckpointEngine.DefaultAdaptiveVisitorCheckpointEngine();
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("code-853", "title-34");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveVisitorCheckpointEngineKeepsResultsWithinBounds();
    }
}
