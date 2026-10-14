namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalRecognitionThresholdEngineChecks
{
    public static void RegionalRecognitionThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalRecognitionThresholdEngine.DefaultRegionalRecognitionThresholdEngine(Flags.NewRegistry());
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("code-150", "label-902");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalRecognitionThresholdEngineKeepsResultsWithinBounds();
    }
}
