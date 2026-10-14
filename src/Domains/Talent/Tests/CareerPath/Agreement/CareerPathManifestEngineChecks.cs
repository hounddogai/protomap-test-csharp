namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathManifestEngineChecks
{
    public static void CareerPathManifestEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathManifestEngine.DefaultCareerPathManifestEngine();
        var item = CareerPathManifest.NewCareerPathManifest("code-597", "title-514");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathManifestEngineKeepsResultsWithinBounds();
    }
}
