namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalSpanEntryStoreChecks
{
    public static void MemoryGlobalSpanEntryStoreFindsWhatItSaved()
    {
        var store = GlobalSpanEntryStore.NewMemoryGlobalSpanEntryStore();
        var item = GlobalSpanEntry.NewGlobalSpanEntry("code-524", "name-922");
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
        MemoryGlobalSpanEntryStoreFindsWhatItSaved();
    }
}
