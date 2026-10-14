namespace Acme.Workforce.Orgdesign.Tests.Headcount.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultHeadcountQuotaEngineChecks
{
    public static void DefaultHeadcountQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultHeadcountQuotaEngine.DefaultDefaultHeadcountQuotaEngine(Flags.NewRegistry());
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("code-241", "name-616");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultHeadcountQuotaEngineKeepsResultsWithinBounds();
    }
}
