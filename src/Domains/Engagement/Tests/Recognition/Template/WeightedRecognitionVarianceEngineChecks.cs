namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WeightedRecognitionVarianceEngineChecks
{
    public static void WeightedRecognitionVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedRecognitionVarianceEngine.DefaultWeightedRecognitionVarianceEngine();
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("code-486", "label-607");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedRecognitionVarianceEngineKeepsResultsWithinBounds();
    }
}
