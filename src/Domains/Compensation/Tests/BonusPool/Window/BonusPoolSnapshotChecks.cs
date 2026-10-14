namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolSnapshotChecks
{
    public static void BonusPoolSnapshotStartsInNew()
    {
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("code-567", "name-611");
        Check.Equal(item.Status, BonusPoolSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolSnapshotMovesToQueued()
    {
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("code-567", "name-611");
        try
        {
            item.MoveTo(BonusPoolSnapshotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolSnapshotStatus.Queued, "status");
    }

    public static void BonusPoolSnapshotRejectsMovesFromFailed()
    {
        var item = BonusPoolSnapshot.NewBonusPoolSnapshot("code-567", "name-611");
        item.Status = BonusPoolSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(BonusPoolSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        BonusPoolSnapshotStartsInNew();
        BonusPoolSnapshotMovesToQueued();
        BonusPoolSnapshotRejectsMovesFromFailed();
    }
}
