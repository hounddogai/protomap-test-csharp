namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkReservationStoreChecks
{
    public static void MemoryBenchmarkReservationStoreFindsWhatItSaved()
    {
        var store = BenchmarkReservationStore.NewMemoryBenchmarkReservationStore();
        var item = BenchmarkReservation.NewBenchmarkReservation("code-424", "title-829");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBenchmarkReservationStoreFindsWhatItSaved();
    }
}
