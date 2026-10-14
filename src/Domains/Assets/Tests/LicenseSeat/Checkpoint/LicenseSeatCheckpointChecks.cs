namespace Acme.Workforce.Assets.Tests.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LicenseSeatCheckpointChecks
{
    public static void LicenseSeatCheckpointStartsInProposed()
    {
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("code-647", "title-43");
        Check.Equal(item.Status, LicenseSeatCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatCheckpointMovesToApproved()
    {
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("code-647", "title-43");
        try
        {
            item.MoveTo(LicenseSeatCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatCheckpointStatus.Approved, "status");
    }

    public static void LicenseSeatCheckpointRejectsMovesFromWithdrawn()
    {
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("code-647", "title-43");
        item.Status = LicenseSeatCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LicenseSeatCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LicenseSeatCheckpointStartsInProposed();
        LicenseSeatCheckpointMovesToApproved();
        LicenseSeatCheckpointRejectsMovesFromWithdrawn();
    }
}
