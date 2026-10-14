namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanQuotaStoreChecks
{
    public static void MemorySpanQuotaStoreFindsWhatItSaved()
    {
        var store = SpanQuotaStore.NewMemorySpanQuotaStore();
        var item = SpanQuota.NewSpanQuota("code-632", "label-861");
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
        MemorySpanQuotaStoreFindsWhatItSaved();
    }
}
