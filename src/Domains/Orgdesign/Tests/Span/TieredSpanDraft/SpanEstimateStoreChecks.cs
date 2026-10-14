namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanEstimateStoreChecks
{
    public static void MemorySpanEstimateStoreFindsWhatItSaved()
    {
        var store = SpanEstimateStore.NewMemorySpanEstimateStore();
        var item = SpanEstimate.NewSpanEstimate("code-978", "name-708");
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
        MemorySpanEstimateStoreFindsWhatItSaved();
    }
}
