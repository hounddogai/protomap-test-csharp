namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TaxFormSegmentEngineChecks
{
    public static void TaxFormSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormSegmentEngine.DefaultTaxFormSegmentEngine(Flags.NewRegistry());
        var item = TaxFormSegment.NewTaxFormSegment("code-115", "name-144");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormSegmentEngineKeepsResultsWithinBounds();
    }
}
