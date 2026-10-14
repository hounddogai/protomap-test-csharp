namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class BatchedMobilityAllocationChecks
{
    public static void BatchedMobilityAllocationStartsInDraft()
    {
        var item = BatchedMobilityAllocation.NewBatchedMobilityAllocation("code-805", "title-322");
        Check.Equal(item.Status, BatchedMobilityAllocationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedMobilityAllocationMovesToSuspended()
    {
        var item = BatchedMobilityAllocation.NewBatchedMobilityAllocation("code-805", "title-322");
        try
        {
            item.MoveTo(BatchedMobilityAllocationStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedMobilityAllocationStatus.Suspended, "status");
    }

    public static void BatchedMobilityAllocationRejectsMovesFromClosed()
    {
        var item = BatchedMobilityAllocation.NewBatchedMobilityAllocation("code-805", "title-322");
        item.Status = BatchedMobilityAllocationStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedMobilityAllocationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedMobilityAllocationStartsInDraft();
        BatchedMobilityAllocationMovesToSuspended();
        BatchedMobilityAllocationRejectsMovesFromClosed();
    }
}
