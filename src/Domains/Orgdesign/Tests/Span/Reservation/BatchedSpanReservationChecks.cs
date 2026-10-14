namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedSpanReservationChecks
{
    public static void BatchedSpanReservationStartsInRequested()
    {
        var item = BatchedSpanReservation.NewBatchedSpanReservation("code-597", "name-319");
        Check.Equal(item.Status, BatchedSpanReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedSpanReservationMovesToGranted()
    {
        var item = BatchedSpanReservation.NewBatchedSpanReservation("code-597", "name-319");
        try
        {
            item.MoveTo(BatchedSpanReservationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedSpanReservationStatus.Granted, "status");
    }

    public static void BatchedSpanReservationRejectsMovesFromExpired()
    {
        var item = BatchedSpanReservation.NewBatchedSpanReservation("code-597", "name-319");
        item.Status = BatchedSpanReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedSpanReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedSpanReservationStartsInRequested();
        BatchedSpanReservationMovesToGranted();
        BatchedSpanReservationRejectsMovesFromExpired();
    }
}
