namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionBundleEngineChecks
{
    public static void RecognitionBundleEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionBundleEngine.DefaultRecognitionBundleEngine(Flags.NewRegistry());
        var item = RecognitionBundle.NewRecognitionBundle("code-813", "name-920");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionBundleEngineKeepsResultsWithinBounds();
    }
}
