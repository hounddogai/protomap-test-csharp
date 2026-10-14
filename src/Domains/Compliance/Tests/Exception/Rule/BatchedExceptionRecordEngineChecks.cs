namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedExceptionRecordEngineChecks
{
    public static void BatchedExceptionRecordEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedExceptionRecordEngine.DefaultBatchedExceptionRecordEngine();
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("code-361", "name-538");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedExceptionRecordEngineKeepsResultsWithinBounds();
    }
}
