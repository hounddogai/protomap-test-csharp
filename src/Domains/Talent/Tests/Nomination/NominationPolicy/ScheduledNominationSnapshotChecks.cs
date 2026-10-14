namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationSnapshotChecks
{
    public static void ScheduledNominationSnapshotStartsInCreated()
    {
        var item = ScheduledNominationSnapshot.NewScheduledNominationSnapshot("code-10", "title-707");
        Check.Equal(item.Status, ScheduledNominationSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledNominationSnapshotMovesToPublished()
    {
        var item = ScheduledNominationSnapshot.NewScheduledNominationSnapshot("code-10", "title-707");
        try
        {
            item.MoveTo(ScheduledNominationSnapshotStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledNominationSnapshotStatus.Published, "status");
    }

    public static void ScheduledNominationSnapshotRejectsMovesFromRetired()
    {
        var item = ScheduledNominationSnapshot.NewScheduledNominationSnapshot("code-10", "title-707");
        item.Status = ScheduledNominationSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledNominationSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledNominationSnapshotStartsInCreated();
        ScheduledNominationSnapshotMovesToPublished();
        ScheduledNominationSnapshotRejectsMovesFromRetired();
    }
}
