namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionCycleEngineChecks
{
    public static void RecognitionCycleEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionCycleEngine.DefaultRecognitionCycleEngine(Flags.NewRegistry());
        var item = RecognitionCycle.NewRecognitionCycle("code-384", "name-558");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionCycleEngineKeepsResultsWithinBounds();
    }
}
