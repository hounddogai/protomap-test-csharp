namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationCheckpointChecks
{
    public static void NominationCheckpointStartsInDraft()
    {
        var item = NominationCheckpoint.NewNominationCheckpoint("code-198", "title-553");
        Check.Equal(item.Status, NominationCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationCheckpointMovesToClosed()
    {
        var item = NominationCheckpoint.NewNominationCheckpoint("code-198", "title-553");
        try
        {
            item.MoveTo(NominationCheckpointStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationCheckpointStatus.Closed, "status");
    }

    public static void NominationCheckpointRejectsMovesFromClosed()
    {
        var item = NominationCheckpoint.NewNominationCheckpoint("code-198", "title-553");
        item.Status = NominationCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(NominationCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        NominationCheckpointStartsInDraft();
        NominationCheckpointMovesToClosed();
        NominationCheckpointRejectsMovesFromClosed();
    }
}
