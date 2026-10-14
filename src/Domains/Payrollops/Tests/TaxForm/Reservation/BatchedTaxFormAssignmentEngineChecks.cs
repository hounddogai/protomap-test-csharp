namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedTaxFormAssignmentEngineChecks
{
    public static void BatchedTaxFormAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedTaxFormAssignmentEngine.DefaultBatchedTaxFormAssignmentEngine(Flags.NewRegistry());
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("code-702", "title-666");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedTaxFormAssignmentEngineKeepsResultsWithinBounds();
    }
}
