namespace Acme.Workforce.Engagement.Tests.Recognition.Digest;

using Acme.Workforce.Engagement.Recognition.Digest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RecognitionSegmentEngineChecks
{
    public static void RecognitionSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionSegmentEngine.DefaultRecognitionSegmentEngine(Flags.NewRegistry());
        var item = RecognitionSegment.NewRecognitionSegment("code-530", "title-102");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionSegmentEngineKeepsResultsWithinBounds();
    }
}
