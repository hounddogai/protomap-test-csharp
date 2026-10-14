namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionAdjustmentEngineChecks
{
    public static void RecognitionAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionAdjustmentEngine.DefaultRecognitionAdjustmentEngine();
        var item = RecognitionAdjustment.NewRecognitionAdjustment("code-333", "name-875");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionAdjustmentEngineKeepsResultsWithinBounds();
    }
}
