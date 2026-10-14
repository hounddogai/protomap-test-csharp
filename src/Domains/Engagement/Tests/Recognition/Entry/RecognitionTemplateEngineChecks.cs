namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionTemplateEngineChecks
{
    public static void RecognitionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionTemplateEngine.DefaultRecognitionTemplateEngine();
        var item = RecognitionTemplate.NewRecognitionTemplate("code-894", "name-483");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionTemplateEngineKeepsResultsWithinBounds();
    }
}
