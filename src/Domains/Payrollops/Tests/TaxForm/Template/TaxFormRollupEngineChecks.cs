namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormRollupEngineChecks
{
    public static void TaxFormRollupEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormRollupEngine.DefaultTaxFormRollupEngine(Flags.NewRegistry());
        var item = TaxFormRollup.NewTaxFormRollup("code-891", "title-149");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormRollupEngineKeepsResultsWithinBounds();
    }
}
