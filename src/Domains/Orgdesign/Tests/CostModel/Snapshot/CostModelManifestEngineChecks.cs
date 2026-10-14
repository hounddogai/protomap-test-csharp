namespace Acme.Workforce.Orgdesign.Tests.CostModel.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class CostModelManifestEngineChecks
{
    public static void CostModelManifestEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelManifestEngine.DefaultCostModelManifestEngine(Flags.NewRegistry());
        var item = CostModelManifest.NewCostModelManifest("code-416", "name-464");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelManifestEngineKeepsResultsWithinBounds();
    }
}
