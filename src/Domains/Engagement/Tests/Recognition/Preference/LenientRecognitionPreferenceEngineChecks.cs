namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientRecognitionPreferenceEngineChecks
{
    public static void LenientRecognitionPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRecognitionPreferenceEngine.DefaultLenientRecognitionPreferenceEngine();
        var item = LenientRecognitionPreference.NewLenientRecognitionPreference("code-889", "title-161");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRecognitionPreferenceEngineKeepsResultsWithinBounds();
    }
}
