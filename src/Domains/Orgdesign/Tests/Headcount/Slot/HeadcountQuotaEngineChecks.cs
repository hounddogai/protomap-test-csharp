namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountQuotaEngineChecks
{
    public static void HeadcountQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountQuotaEngine.DefaultHeadcountQuotaEngine();
        var item = HeadcountQuota.NewHeadcountQuota("code-655", "label-859");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountQuotaEngineKeepsResultsWithinBounds();
    }
}
