namespace Acme.Workforce.Orgdesign.Tests.CostModel.Snapshot;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class CostModelSnapshotChecks
{
    public static void CostModelSnapshotStartsInRequested()
    {
        var item = CostModelSnapshot.NewCostModelSnapshot("code-978", "title-224");
        Check.Equal(item.Status, CostModelSnapshotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelSnapshotMovesToExpired()
    {
        var item = CostModelSnapshot.NewCostModelSnapshot("code-978", "title-224");
        try
        {
            item.MoveTo(CostModelSnapshotStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelSnapshotStatus.Expired, "status");
    }

    public static void CostModelSnapshotRejectsMovesFromExpired()
    {
        var item = CostModelSnapshot.NewCostModelSnapshot("code-978", "title-224");
        item.Status = CostModelSnapshotStatus.Expired;
        Check.Fails(() => item.MoveTo(CostModelSnapshotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CostModelSnapshotStartsInRequested();
        CostModelSnapshotMovesToExpired();
        CostModelSnapshotRejectsMovesFromExpired();
    }
}
