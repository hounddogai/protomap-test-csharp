namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanProfileStoreChecks
{
    public static void MemorySpanProfileStoreFindsWhatItSaved()
    {
        var store = SpanProfileStore.NewMemorySpanProfileStore();
        var item = SpanProfile.NewSpanProfile("code-753", "label-297");
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
        MemorySpanProfileStoreFindsWhatItSaved();
    }
}
