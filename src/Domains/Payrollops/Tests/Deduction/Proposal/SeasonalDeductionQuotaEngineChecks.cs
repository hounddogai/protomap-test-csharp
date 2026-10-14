namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalDeductionQuotaEngineChecks
{
    public static void SeasonalDeductionQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalDeductionQuotaEngine.DefaultSeasonalDeductionQuotaEngine(Flags.NewRegistry());
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("code-242", "label-335");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalDeductionQuotaEngineKeepsResultsWithinBounds();
    }
}
