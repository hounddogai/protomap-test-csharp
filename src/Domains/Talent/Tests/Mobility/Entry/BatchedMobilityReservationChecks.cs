namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedMobilityReservationChecks
{
    public static void BatchedMobilityReservationStartsInRequested()
    {
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("code-42", "title-255");
        Check.Equal(item.Status, BatchedMobilityReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedMobilityReservationMovesToGranted()
    {
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("code-42", "title-255");
        try
        {
            item.MoveTo(BatchedMobilityReservationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedMobilityReservationStatus.Granted, "status");
    }

    public static void BatchedMobilityReservationRejectsMovesFromExpired()
    {
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("code-42", "title-255");
        item.Status = BatchedMobilityReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedMobilityReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedMobilityReservationStartsInRequested();
        BatchedMobilityReservationMovesToGranted();
        BatchedMobilityReservationRejectsMovesFromExpired();
    }
}
