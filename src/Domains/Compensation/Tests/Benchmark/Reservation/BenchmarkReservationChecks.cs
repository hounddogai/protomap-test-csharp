namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkReservationChecks
{
    public static void BenchmarkReservationStartsInDraft()
    {
        var item = BenchmarkReservation.NewBenchmarkReservation("code-424", "title-829");
        Check.Equal(item.Status, BenchmarkReservationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkReservationMovesToSuspended()
    {
        var item = BenchmarkReservation.NewBenchmarkReservation("code-424", "title-829");
        try
        {
            item.MoveTo(BenchmarkReservationStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkReservationStatus.Suspended, "status");
    }

    public static void BenchmarkReservationRejectsMovesFromClosed()
    {
        var item = BenchmarkReservation.NewBenchmarkReservation("code-424", "title-829");
        item.Status = BenchmarkReservationStatus.Closed;
        Check.Fails(() => item.MoveTo(BenchmarkReservationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BenchmarkReservationStartsInDraft();
        BenchmarkReservationMovesToSuspended();
        BenchmarkReservationRejectsMovesFromClosed();
    }
}
