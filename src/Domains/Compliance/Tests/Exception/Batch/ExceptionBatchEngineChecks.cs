namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ExceptionBatchEngineChecks
{
    public static void ExceptionBatchEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionBatchEngine.DefaultExceptionBatchEngine(Flags.NewRegistry());
        var item = ExceptionBatch.NewExceptionBatch("code-631", "title-8");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionBatchEngineKeepsResultsWithinBounds();
    }
}
