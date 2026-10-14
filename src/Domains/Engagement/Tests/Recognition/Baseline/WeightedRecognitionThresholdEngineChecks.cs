namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedRecognitionThresholdEngineChecks
{
    public static void WeightedRecognitionThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedRecognitionThresholdEngine.DefaultWeightedRecognitionThresholdEngine(Flags.NewRegistry());
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("code-809", "label-43");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedRecognitionThresholdEngineKeepsResultsWithinBounds();
    }
}
