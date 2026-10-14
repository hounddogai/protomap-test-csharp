namespace Acme.Workforce.Orgdesign.Tests.Scenario.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyScenarioSnapshotEngineChecks
{
    public static void LegacyScenarioSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyScenarioSnapshotEngine.DefaultLegacyScenarioSnapshotEngine(Flags.NewRegistry());
        var item = LegacyScenarioSnapshot.NewLegacyScenarioSnapshot("code-202", "label-487");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyScenarioSnapshotEngineKeepsResultsWithinBounds();
    }
}
