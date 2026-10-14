namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanRollupStoreChecks
{
    public static void MemorySpanRollupStoreFindsWhatItSaved()
    {
        var store = SpanRollupStore.NewMemorySpanRollupStore();
        var item = SpanRollup.NewSpanRollup("code-807", "name-805");
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
        MemorySpanRollupStoreFindsWhatItSaved();
    }
}
