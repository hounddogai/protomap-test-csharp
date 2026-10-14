namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LegacyHeadcountQuotaEngineChecks
{
    public static void LegacyHeadcountQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyHeadcountQuotaEngine.DefaultLegacyHeadcountQuotaEngine();
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("code-743", "label-929");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyHeadcountQuotaEngineKeepsResultsWithinBounds();
    }
}
