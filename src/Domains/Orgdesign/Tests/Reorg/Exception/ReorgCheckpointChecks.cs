namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgCheckpointChecks
{
    public static void ReorgCheckpointStartsInDraft()
    {
        var item = ReorgCheckpoint.NewReorgCheckpoint("code-880", "name-406");
        Check.Equal(item.Status, ReorgCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgCheckpointMovesToSuspended()
    {
        var item = ReorgCheckpoint.NewReorgCheckpoint("code-880", "name-406");
        try
        {
            item.MoveTo(ReorgCheckpointStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgCheckpointStatus.Suspended, "status");
    }

    public static void ReorgCheckpointRejectsMovesFromClosed()
    {
        var item = ReorgCheckpoint.NewReorgCheckpoint("code-880", "name-406");
        item.Status = ReorgCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(ReorgCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ReorgCheckpointStartsInDraft();
        ReorgCheckpointMovesToSuspended();
        ReorgCheckpointRejectsMovesFromClosed();
    }
}
