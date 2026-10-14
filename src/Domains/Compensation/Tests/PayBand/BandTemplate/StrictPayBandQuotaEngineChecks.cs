namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class StrictPayBandQuotaEngineChecks
{
    public static void StrictPayBandQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPayBandQuotaEngine.DefaultStrictPayBandQuotaEngine(Flags.NewRegistry());
        var item = StrictPayBandQuota.NewStrictPayBandQuota("code-317", "label-505");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPayBandQuotaEngineKeepsResultsWithinBounds();
    }
}
