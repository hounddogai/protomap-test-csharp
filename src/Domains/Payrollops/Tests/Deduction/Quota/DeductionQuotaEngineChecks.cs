namespace Acme.Workforce.Payrollops.Tests.Deduction.Quota;

using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DeductionQuotaEngineChecks
{
    public static void DeductionQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionQuotaEngine.DefaultDeductionQuotaEngine();
        var item = DeductionQuota.NewDeductionQuota("code-12", "name-925");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionQuotaEngineKeepsResultsWithinBounds();
    }
}
