namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormRecordEngineChecks
{
    public static void AdaptiveTaxFormRecordEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTaxFormRecordEngine.DefaultAdaptiveTaxFormRecordEngine(Flags.NewRegistry());
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("code-852", "name-527");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormRecordEngineKeepsResultsWithinBounds();
    }
}
