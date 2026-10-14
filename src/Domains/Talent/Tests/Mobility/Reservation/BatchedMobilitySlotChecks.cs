namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedMobilitySlotChecks
{
    public static void BatchedMobilitySlotStartsInDraft()
    {
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("code-67", "title-523");
        Check.Equal(item.Status, BatchedMobilitySlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedMobilitySlotMovesToActive()
    {
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("code-67", "title-523");
        try
        {
            item.MoveTo(BatchedMobilitySlotStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedMobilitySlotStatus.Active, "status");
    }

    public static void BatchedMobilitySlotRejectsMovesFromClosed()
    {
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("code-67", "title-523");
        item.Status = BatchedMobilitySlotStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedMobilitySlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedMobilitySlotStartsInDraft();
        BatchedMobilitySlotMovesToActive();
        BatchedMobilitySlotRejectsMovesFromClosed();
    }
}
