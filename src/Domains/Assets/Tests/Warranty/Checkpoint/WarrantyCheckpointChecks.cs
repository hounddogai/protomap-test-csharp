namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyCheckpointChecks
{
    public static void WarrantyCheckpointStartsInPlanned()
    {
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("code-223", "title-507");
        Check.Equal(item.Status, WarrantyCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyCheckpointMovesToScheduled()
    {
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("code-223", "title-507");
        try
        {
            item.MoveTo(WarrantyCheckpointStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyCheckpointStatus.Scheduled, "status");
    }

    public static void WarrantyCheckpointRejectsMovesFromCancelled()
    {
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("code-223", "title-507");
        item.Status = WarrantyCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WarrantyCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WarrantyCheckpointStartsInPlanned();
        WarrantyCheckpointMovesToScheduled();
        WarrantyCheckpointRejectsMovesFromCancelled();
    }
}
