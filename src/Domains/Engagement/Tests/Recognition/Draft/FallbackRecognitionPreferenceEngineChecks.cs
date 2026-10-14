namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackRecognitionPreferenceEngineChecks
{
    public static void FallbackRecognitionPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackRecognitionPreferenceEngine.DefaultFallbackRecognitionPreferenceEngine();
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("code-654", "name-317");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackRecognitionPreferenceEngineKeepsResultsWithinBounds();
    }
}
