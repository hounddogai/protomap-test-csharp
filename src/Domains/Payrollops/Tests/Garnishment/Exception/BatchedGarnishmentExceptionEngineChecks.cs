namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedGarnishmentExceptionEngineChecks
{
    public static void BatchedGarnishmentExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedGarnishmentExceptionEngine.DefaultBatchedGarnishmentExceptionEngine(Flags.NewRegistry());
        var item = BatchedGarnishmentException.NewBatchedGarnishmentException("code-274", "title-694");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedGarnishmentExceptionEngineKeepsResultsWithinBounds();
    }
}
