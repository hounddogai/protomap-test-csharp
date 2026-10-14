namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionCheckpointEngineChecks
{
    public static void RecognitionCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionCheckpointEngine.DefaultRecognitionCheckpointEngine(Flags.NewRegistry());
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("code-157", "title-508");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionCheckpointEngineKeepsResultsWithinBounds();
    }
}
