namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ManualRecognitionSegmentEngineChecks
{
    public static void ManualRecognitionSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = ManualRecognitionSegmentEngine.DefaultManualRecognitionSegmentEngine();
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("code-70", "title-462");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualRecognitionSegmentEngineKeepsResultsWithinBounds();
    }
}
