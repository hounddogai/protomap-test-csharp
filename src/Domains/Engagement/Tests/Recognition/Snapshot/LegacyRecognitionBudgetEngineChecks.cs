namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyRecognitionBudgetEngineChecks
{
    public static void LegacyRecognitionBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyRecognitionBudgetEngine.DefaultLegacyRecognitionBudgetEngine();
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("code-878", "name-75");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyRecognitionBudgetEngineKeepsResultsWithinBounds();
    }
}
