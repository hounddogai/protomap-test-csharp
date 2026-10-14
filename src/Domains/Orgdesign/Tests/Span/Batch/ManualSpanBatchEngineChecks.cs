namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualSpanBatchEngineChecks
{
    public static void ManualSpanBatchEngineKeepsResultsWithinBounds()
    {
        var engine = ManualSpanBatchEngine.DefaultManualSpanBatchEngine();
        var item = ManualSpanBatch.NewManualSpanBatch("code-386", "title-448");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualSpanBatchEngineKeepsResultsWithinBounds();
    }
}
