namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientRecognitionBatchEngineChecks
{
    public static void LenientRecognitionBatchEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRecognitionBatchEngine.DefaultLenientRecognitionBatchEngine();
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("code-578", "label-370");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRecognitionBatchEngineKeepsResultsWithinBounds();
    }
}
