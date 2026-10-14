namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class CappedTaxFormAssignmentEngineChecks
{
    public static void CappedTaxFormAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedTaxFormAssignmentEngine.DefaultCappedTaxFormAssignmentEngine(Flags.NewRegistry());
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("code-253", "title-778");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedTaxFormAssignmentEngineKeepsResultsWithinBounds();
    }
}
