namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedRecognitionDraftEngineChecks
{
    public static void BatchedRecognitionDraftEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedRecognitionDraftEngine.DefaultBatchedRecognitionDraftEngine(Flags.NewRegistry());
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("code-802", "name-202");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedRecognitionDraftEngineKeepsResultsWithinBounds();
    }
}
