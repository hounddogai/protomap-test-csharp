namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSnapshotChecks
{
    public static void AdjustmentSnapshotStartsInOpen()
    {
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("code-284", "label-925");
        Check.Equal(item.Status, AdjustmentSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentSnapshotMovesToReopened()
    {
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("code-284", "label-925");
        try
        {
            item.MoveTo(AdjustmentSnapshotStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentSnapshotStatus.Reopened, "status");
    }

    public static void AdjustmentSnapshotRejectsMovesFromArchived()
    {
        var item = AdjustmentSnapshot.NewAdjustmentSnapshot("code-284", "label-925");
        item.Status = AdjustmentSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(AdjustmentSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdjustmentSnapshotStartsInOpen();
        AdjustmentSnapshotMovesToReopened();
        AdjustmentSnapshotRejectsMovesFromArchived();
    }
}
