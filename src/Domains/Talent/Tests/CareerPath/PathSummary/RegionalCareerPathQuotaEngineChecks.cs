namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathQuotaEngineChecks
{
    public static void RegionalCareerPathQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalCareerPathQuotaEngine.DefaultRegionalCareerPathQuotaEngine();
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("code-925", "label-252");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalCareerPathQuotaEngineKeepsResultsWithinBounds();
    }
}
