namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class FallbackRecognitionBaselineEngineChecks
{
    public static void FallbackRecognitionBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackRecognitionBaselineEngine.DefaultFallbackRecognitionBaselineEngine();
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("code-827", "name-523");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackRecognitionBaselineEngineKeepsResultsWithinBounds();
    }
}
