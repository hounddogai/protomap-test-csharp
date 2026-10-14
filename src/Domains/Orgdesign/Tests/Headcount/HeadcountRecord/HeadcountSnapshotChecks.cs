namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class HeadcountSnapshotChecks
{
    public static void HeadcountSnapshotStartsInIdle()
    {
        var item = HeadcountSnapshot.NewHeadcountSnapshot("code-96", "label-172");
        Check.Equal(item.Status, HeadcountSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountSnapshotMovesToDraining()
    {
        var item = HeadcountSnapshot.NewHeadcountSnapshot("code-96", "label-172");
        try
        {
            item.MoveTo(HeadcountSnapshotStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountSnapshotStatus.Draining, "status");
    }

    public static void HeadcountSnapshotRejectsMovesFromStopped()
    {
        var item = HeadcountSnapshot.NewHeadcountSnapshot("code-96", "label-172");
        item.Status = HeadcountSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(HeadcountSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HeadcountSnapshotStartsInIdle();
        HeadcountSnapshotMovesToDraining();
        HeadcountSnapshotRejectsMovesFromStopped();
    }
}
