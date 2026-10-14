namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveRecognitionBatchEngineChecks
{
    public static void AdaptiveRecognitionBatchEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveRecognitionBatchEngine.DefaultAdaptiveRecognitionBatchEngine();
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("code-203", "label-860");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveRecognitionBatchEngineKeepsResultsWithinBounds();
    }
}
