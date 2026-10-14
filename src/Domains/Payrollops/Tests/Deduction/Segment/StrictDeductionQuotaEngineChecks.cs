namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionQuotaEngineChecks
{
    public static void StrictDeductionQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = StrictDeductionQuotaEngine.DefaultStrictDeductionQuotaEngine(Flags.NewRegistry());
        var item = StrictDeductionQuota.NewStrictDeductionQuota("code-365", "label-753");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictDeductionQuotaEngineKeepsResultsWithinBounds();
    }
}
