namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictRecognitionCycleEngineChecks
{
    public static void StrictRecognitionCycleEngineKeepsResultsWithinBounds()
    {
        var engine = StrictRecognitionCycleEngine.DefaultStrictRecognitionCycleEngine(Flags.NewRegistry());
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("code-942", "name-212");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictRecognitionCycleEngineKeepsResultsWithinBounds();
    }
}
