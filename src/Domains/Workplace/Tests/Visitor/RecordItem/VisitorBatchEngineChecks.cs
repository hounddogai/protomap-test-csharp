namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorBatchEngineChecks
{
    public static void VisitorBatchEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorBatchEngine.DefaultVisitorBatchEngine();
        var item = VisitorBatch.NewVisitorBatch("code-361", "title-289");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorBatchEngineKeepsResultsWithinBounds();
    }
}
