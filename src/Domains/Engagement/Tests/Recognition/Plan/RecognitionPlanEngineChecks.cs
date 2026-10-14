namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionPlanEngineChecks
{
    public static void RecognitionPlanEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionPlanEngine.DefaultRecognitionPlanEngine(Flags.NewRegistry());
        var item = RecognitionPlan.NewRecognitionPlan("code-574", "title-883");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionPlanEngineKeepsResultsWithinBounds();
    }
}
