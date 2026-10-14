namespace Acme.Workforce.Payrollops.Tests.TaxForm.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TaxFormTargetEngineChecks
{
    public static void TaxFormTargetEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormTargetEngine.DefaultTaxFormTargetEngine(Flags.NewRegistry());
        var item = TaxFormTarget.NewTaxFormTarget("code-679", "title-912");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormTargetEngineKeepsResultsWithinBounds();
    }
}
