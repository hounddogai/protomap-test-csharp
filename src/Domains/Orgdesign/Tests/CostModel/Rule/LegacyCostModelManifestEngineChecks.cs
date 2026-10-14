namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyCostModelManifestEngineChecks
{
    public static void LegacyCostModelManifestEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyCostModelManifestEngine.DefaultLegacyCostModelManifestEngine();
        var item = LegacyCostModelManifest.NewLegacyCostModelManifest("code-910", "title-805");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyCostModelManifestEngineKeepsResultsWithinBounds();
    }
}
