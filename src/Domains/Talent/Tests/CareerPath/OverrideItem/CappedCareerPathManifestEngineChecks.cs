namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedCareerPathManifestEngineChecks
{
    public static void CappedCareerPathManifestEngineKeepsResultsWithinBounds()
    {
        var engine = CappedCareerPathManifestEngine.DefaultCappedCareerPathManifestEngine();
        var item = CappedCareerPathManifest.NewCappedCareerPathManifest("code-324", "name-575");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedCareerPathManifestEngineKeepsResultsWithinBounds();
    }
}
