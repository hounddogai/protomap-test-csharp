namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalPayBandQuotaEngineChecks
{
    public static void SeasonalPayBandQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPayBandQuotaEngine.DefaultSeasonalPayBandQuotaEngine();
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("code-589", "label-24");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPayBandQuotaEngineKeepsResultsWithinBounds();
    }
}
