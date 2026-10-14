namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RecognitionMetricEngineChecks
{
    public static void RecognitionMetricEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionMetricEngine.DefaultRecognitionMetricEngine(Flags.NewRegistry());
        var item = RecognitionMetric.NewRecognitionMetric("code-746", "name-600");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionMetricEngineKeepsResultsWithinBounds();
    }
}
