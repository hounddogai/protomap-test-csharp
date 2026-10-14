namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RollingBenchmarkProposalStoreChecks
{
    public static void MemoryRollingBenchmarkProposalStoreFindsWhatItSaved()
    {
        var store = RollingBenchmarkProposalStore.NewMemoryRollingBenchmarkProposalStore();
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("code-387", "name-560");
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
        MemoryRollingBenchmarkProposalStoreFindsWhatItSaved();
    }
}
