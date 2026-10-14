namespace Acme.Workforce.Orgdesign.Tests.Scenario.Snapshot;

using Acme.Workforce.Orgdesign.Scenario.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyScenarioSnapshotChecks
{
    public static void LegacyScenarioSnapshotStartsInNew()
    {
        var item = LegacyScenarioSnapshot.NewLegacyScenarioSnapshot("code-202", "label-487");
        Check.Equal(item.Status, LegacyScenarioSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyScenarioSnapshotMovesToRunning()
    {
        var item = LegacyScenarioSnapshot.NewLegacyScenarioSnapshot("code-202", "label-487");
        try
        {
            item.MoveTo(LegacyScenarioSnapshotStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyScenarioSnapshotStatus.Running, "status");
    }

    public static void LegacyScenarioSnapshotRejectsMovesFromFailed()
    {
        var item = LegacyScenarioSnapshot.NewLegacyScenarioSnapshot("code-202", "label-487");
        item.Status = LegacyScenarioSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyScenarioSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyScenarioSnapshotStartsInNew();
        LegacyScenarioSnapshotMovesToRunning();
        LegacyScenarioSnapshotRejectsMovesFromFailed();
    }
}
