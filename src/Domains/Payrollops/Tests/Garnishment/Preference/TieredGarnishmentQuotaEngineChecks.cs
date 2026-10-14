namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredGarnishmentQuotaEngineChecks
{
    public static void TieredGarnishmentQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = TieredGarnishmentQuotaEngine.DefaultTieredGarnishmentQuotaEngine(Flags.NewRegistry());
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("code-691", "title-669");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredGarnishmentQuotaEngineKeepsResultsWithinBounds();
    }
}
