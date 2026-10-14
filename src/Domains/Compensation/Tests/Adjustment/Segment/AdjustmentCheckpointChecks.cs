namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentCheckpointChecks
{
    public static void AdjustmentCheckpointStartsInProposed()
    {
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("code-317", "name-296");
        Check.Equal(item.Status, AdjustmentCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentCheckpointMovesToRejected()
    {
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("code-317", "name-296");
        try
        {
            item.MoveTo(AdjustmentCheckpointStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentCheckpointStatus.Rejected, "status");
    }

    public static void AdjustmentCheckpointRejectsMovesFromWithdrawn()
    {
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("code-317", "name-296");
        item.Status = AdjustmentCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdjustmentCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdjustmentCheckpointStartsInProposed();
        AdjustmentCheckpointMovesToRejected();
        AdjustmentCheckpointRejectsMovesFromWithdrawn();
    }
}
