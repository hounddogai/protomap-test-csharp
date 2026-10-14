namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class LenientTaxFormAssignmentEngineChecks
{
    public static void LenientTaxFormAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = LenientTaxFormAssignmentEngine.DefaultLenientTaxFormAssignmentEngine(Flags.NewRegistry());
        var item = LenientTaxFormAssignment.NewLenientTaxFormAssignment("code-218", "title-116");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientTaxFormAssignmentEngineKeepsResultsWithinBounds();
    }
}
