namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SpanGroupStoreChecks
{
    public static void MemorySpanGroupStoreFindsWhatItSaved()
    {
        var store = SpanGroupStore.NewMemorySpanGroupStore();
        var item = SpanGroup.NewSpanGroup("code-261", "label-737");
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
        MemorySpanGroupStoreFindsWhatItSaved();
    }
}
