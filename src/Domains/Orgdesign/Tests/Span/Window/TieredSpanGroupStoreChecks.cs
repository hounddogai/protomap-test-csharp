namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredSpanGroupStoreChecks
{
    public static void MemoryTieredSpanGroupStoreFindsWhatItSaved()
    {
        var store = TieredSpanGroupStore.NewMemoryTieredSpanGroupStore();
        var item = TieredSpanGroup.NewTieredSpanGroup("code-839", "title-177");
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
        MemoryTieredSpanGroupStoreFindsWhatItSaved();
    }
}
