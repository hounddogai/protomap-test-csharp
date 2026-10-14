namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalCareerPathDigestEngineChecks
{
    public static void SeasonalCareerPathDigestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalCareerPathDigestEngine.DefaultSeasonalCareerPathDigestEngine();
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("code-972", "label-577");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalCareerPathDigestEngineKeepsResultsWithinBounds();
    }
}
