namespace Acme.Workforce.Engagement.Tests.Sentiment.Plan;

using Acme.Workforce.Engagement.Sentiment.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingSentimentPlanEngineChecks
{
    public static void RollingSentimentPlanEngineKeepsResultsWithinBounds()
    {
        var engine = RollingSentimentPlanEngine.DefaultRollingSentimentPlanEngine(Flags.NewRegistry());
        var item = RollingSentimentPlan.NewRollingSentimentPlan("code-890", "name-65");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingSentimentPlanEngineKeepsResultsWithinBounds();
    }
}
