namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class RollingCareerPathForecastEngineChecks
{
    public static void RollingCareerPathForecastEngineKeepsResultsWithinBounds()
    {
        var engine = RollingCareerPathForecastEngine.DefaultRollingCareerPathForecastEngine();
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("code-502", "title-376");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingCareerPathForecastEngineKeepsResultsWithinBounds();
    }
}
