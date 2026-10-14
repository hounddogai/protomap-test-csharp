namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredCareerPathOverrideEngineChecks
{
    public static void TieredCareerPathOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = TieredCareerPathOverrideEngine.DefaultTieredCareerPathOverrideEngine();
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("code-643", "label-595");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredCareerPathOverrideEngineKeepsResultsWithinBounds();
    }
}
