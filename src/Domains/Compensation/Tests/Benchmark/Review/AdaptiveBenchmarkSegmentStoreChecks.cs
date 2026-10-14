namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkSegmentStoreChecks
{
    public static void MemoryAdaptiveBenchmarkSegmentStoreFindsWhatItSaved()
    {
        var store = AdaptiveBenchmarkSegmentStore.NewMemoryAdaptiveBenchmarkSegmentStore();
        var item = AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment("code-947", "name-313");
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
        MemoryAdaptiveBenchmarkSegmentStoreFindsWhatItSaved();
    }
}
