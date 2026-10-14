namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormScoreEngineChecks
{
    public static void TaxFormScoreEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormScoreEngine.DefaultTaxFormScoreEngine(Flags.NewRegistry());
        var item = TaxFormScore.NewTaxFormScore("code-276", "label-659");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormScoreEngineKeepsResultsWithinBounds();
    }
}
