namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantySnapshotChecks
{
    public static void WarrantySnapshotStartsInIdle()
    {
        var item = WarrantySnapshot.NewWarrantySnapshot("code-576", "label-767");
        Check.Equal(item.Status, WarrantySnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantySnapshotMovesToWarming()
    {
        var item = WarrantySnapshot.NewWarrantySnapshot("code-576", "label-767");
        try
        {
            item.MoveTo(WarrantySnapshotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantySnapshotStatus.Warming, "status");
    }

    public static void WarrantySnapshotRejectsMovesFromStopped()
    {
        var item = WarrantySnapshot.NewWarrantySnapshot("code-576", "label-767");
        item.Status = WarrantySnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantySnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantySnapshotStartsInIdle();
        WarrantySnapshotMovesToWarming();
        WarrantySnapshotRejectsMovesFromStopped();
    }
}
