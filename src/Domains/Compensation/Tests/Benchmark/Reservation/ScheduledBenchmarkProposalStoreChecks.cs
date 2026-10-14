namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledBenchmarkProposalStoreChecks
{
    public static void MemoryScheduledBenchmarkProposalStoreFindsWhatItSaved()
    {
        var store = ScheduledBenchmarkProposalStore.NewMemoryScheduledBenchmarkProposalStore();
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("code-743", "title-987");
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
        MemoryScheduledBenchmarkProposalStoreFindsWhatItSaved();
    }
}
