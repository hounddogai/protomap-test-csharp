namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkReservationViewChecks
{
    public static void BenchmarkReservationViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkReservation.NewBenchmarkReservation("code-424", "title-829");
        var view = BenchmarkReservationView.ToBenchmarkReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkReservationView.ToBenchmarkReservationViews(new List<BenchmarkReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkReservationViewShowsTheStatusInCapitals();
    }
}
