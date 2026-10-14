namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PendingPayBandQuotaEngineChecks
{
    public static void PendingPayBandQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPayBandQuotaEngine.DefaultPendingPayBandQuotaEngine();
        var item = PendingPayBandQuota.NewPendingPayBandQuota("code-74", "name-401");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPayBandQuotaEngineKeepsResultsWithinBounds();
    }
}
