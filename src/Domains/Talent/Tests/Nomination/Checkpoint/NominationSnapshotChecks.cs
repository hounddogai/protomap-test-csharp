namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationSnapshotChecks
{
    public static void NominationSnapshotStartsInDraft()
    {
        var item = NominationSnapshot.NewNominationSnapshot("code-598", "name-450");
        Check.Equal(item.Status, NominationSnapshotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationSnapshotMovesToSuspended()
    {
        var item = NominationSnapshot.NewNominationSnapshot("code-598", "name-450");
        try
        {
            item.MoveTo(NominationSnapshotStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationSnapshotStatus.Suspended, "status");
    }

    public static void NominationSnapshotRejectsMovesFromClosed()
    {
        var item = NominationSnapshot.NewNominationSnapshot("code-598", "name-450");
        item.Status = NominationSnapshotStatus.Closed;
        Check.Fails(() => item.MoveTo(NominationSnapshotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        NominationSnapshotStartsInDraft();
        NominationSnapshotMovesToSuspended();
        NominationSnapshotRejectsMovesFromClosed();
    }
}
