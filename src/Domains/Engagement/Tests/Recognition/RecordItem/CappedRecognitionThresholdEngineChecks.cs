namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedRecognitionThresholdEngineChecks
{
    public static void CappedRecognitionThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = CappedRecognitionThresholdEngine.DefaultCappedRecognitionThresholdEngine();
        var item = CappedRecognitionThreshold.NewCappedRecognitionThreshold("code-364", "title-193");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedRecognitionThresholdEngineKeepsResultsWithinBounds();
    }
}
