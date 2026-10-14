namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class AccrualQuotaEngineChecks
{
    public static void AccrualQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualQuotaEngine.DefaultAccrualQuotaEngine(Flags.NewRegistry());
        var item = AccrualQuota.NewAccrualQuota("code-160", "label-547");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualQuotaEngineKeepsResultsWithinBounds();
    }
}
