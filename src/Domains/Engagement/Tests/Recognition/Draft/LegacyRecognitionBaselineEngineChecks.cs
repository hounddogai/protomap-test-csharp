namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyRecognitionBaselineEngineChecks
{
    public static void LegacyRecognitionBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyRecognitionBaselineEngine.DefaultLegacyRecognitionBaselineEngine(Flags.NewRegistry());
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("code-462", "name-24");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyRecognitionBaselineEngineKeepsResultsWithinBounds();
    }
}
