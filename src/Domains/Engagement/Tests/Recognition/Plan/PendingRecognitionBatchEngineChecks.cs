namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingRecognitionBatchEngineChecks
{
    public static void PendingRecognitionBatchEngineKeepsResultsWithinBounds()
    {
        var engine = PendingRecognitionBatchEngine.DefaultPendingRecognitionBatchEngine(Flags.NewRegistry());
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("code-835", "title-732");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingRecognitionBatchEngineKeepsResultsWithinBounds();
    }
}
