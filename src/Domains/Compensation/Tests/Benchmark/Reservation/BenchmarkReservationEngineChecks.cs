namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkReservationEngineChecks
{
    public static void BenchmarkReservationEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkReservationEngine.DefaultBenchmarkReservationEngine();
        var item = BenchmarkReservation.NewBenchmarkReservation("code-424", "title-829");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkReservationEngineKeepsResultsWithinBounds();
    }
}
