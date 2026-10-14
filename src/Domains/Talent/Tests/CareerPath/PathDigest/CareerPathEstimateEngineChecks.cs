namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class CareerPathEstimateEngineChecks
{
    public static void CareerPathEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathEstimateEngine.DefaultCareerPathEstimateEngine();
        var item = CareerPathEstimate.NewCareerPathEstimate("code-680", "name-611");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathEstimateEngineKeepsResultsWithinBounds();
    }
}
