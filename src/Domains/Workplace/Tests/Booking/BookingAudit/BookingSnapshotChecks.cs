namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class BookingSnapshotChecks
{
    public static void BookingSnapshotStartsInNew()
    {
        var item = BookingSnapshot.NewBookingSnapshot("code-766", "name-996");
        Check.Equal(item.Status, BookingSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingSnapshotMovesToRunning()
    {
        var item = BookingSnapshot.NewBookingSnapshot("code-766", "name-996");
        try
        {
            item.MoveTo(BookingSnapshotStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingSnapshotStatus.Running, "status");
    }

    public static void BookingSnapshotRejectsMovesFromFailed()
    {
        var item = BookingSnapshot.NewBookingSnapshot("code-766", "name-996");
        item.Status = BookingSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(BookingSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        BookingSnapshotStartsInNew();
        BookingSnapshotMovesToRunning();
        BookingSnapshotRejectsMovesFromFailed();
    }
}
