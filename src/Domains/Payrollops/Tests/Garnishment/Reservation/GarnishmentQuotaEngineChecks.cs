namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GarnishmentQuotaEngineChecks
{
    public static void GarnishmentQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentQuotaEngine.DefaultGarnishmentQuotaEngine();
        var item = GarnishmentQuota.NewGarnishmentQuota("code-885", "name-500");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentQuotaEngineKeepsResultsWithinBounds();
    }
}
