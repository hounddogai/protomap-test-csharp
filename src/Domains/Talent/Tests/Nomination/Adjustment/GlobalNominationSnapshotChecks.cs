namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalNominationSnapshotChecks
{
    public static void GlobalNominationSnapshotStartsInNew()
    {
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("code-672", "title-438");
        Check.Equal(item.Status, GlobalNominationSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalNominationSnapshotMovesToQueued()
    {
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("code-672", "title-438");
        try
        {
            item.MoveTo(GlobalNominationSnapshotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalNominationSnapshotStatus.Queued, "status");
    }

    public static void GlobalNominationSnapshotRejectsMovesFromFailed()
    {
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("code-672", "title-438");
        item.Status = GlobalNominationSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalNominationSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalNominationSnapshotStartsInNew();
        GlobalNominationSnapshotMovesToQueued();
        GlobalNominationSnapshotRejectsMovesFromFailed();
    }
}
