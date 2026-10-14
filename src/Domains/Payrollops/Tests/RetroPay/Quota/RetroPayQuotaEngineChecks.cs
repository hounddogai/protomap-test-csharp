namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayQuotaEngineChecks
{
    public static void RetroPayQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayQuotaEngine.DefaultRetroPayQuotaEngine();
        var item = RetroPayQuota.NewRetroPayQuota("code-261", "name-747");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayQuotaEngineKeepsResultsWithinBounds();
    }
}
