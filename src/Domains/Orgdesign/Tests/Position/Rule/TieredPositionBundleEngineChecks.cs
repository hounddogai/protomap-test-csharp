namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredPositionBundleEngineChecks
{
    public static void TieredPositionBundleEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPositionBundleEngine.DefaultTieredPositionBundleEngine();
        var item = TieredPositionBundle.NewTieredPositionBundle("code-28", "name-162");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPositionBundleEngineKeepsResultsWithinBounds();
    }
}
