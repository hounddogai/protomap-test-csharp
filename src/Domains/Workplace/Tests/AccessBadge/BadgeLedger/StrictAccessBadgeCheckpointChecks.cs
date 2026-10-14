namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class StrictAccessBadgeCheckpointChecks
{
    public static void StrictAccessBadgeCheckpointStartsInRequested()
    {
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("code-11", "name-889");
        Check.Equal(item.Status, StrictAccessBadgeCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAccessBadgeCheckpointMovesToGranted()
    {
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("code-11", "name-889");
        try
        {
            item.MoveTo(StrictAccessBadgeCheckpointStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAccessBadgeCheckpointStatus.Granted, "status");
    }

    public static void StrictAccessBadgeCheckpointRejectsMovesFromExpired()
    {
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("code-11", "name-889");
        item.Status = StrictAccessBadgeCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictAccessBadgeCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictAccessBadgeCheckpointStartsInRequested();
        StrictAccessBadgeCheckpointMovesToGranted();
        StrictAccessBadgeCheckpointRejectsMovesFromExpired();
    }
}
