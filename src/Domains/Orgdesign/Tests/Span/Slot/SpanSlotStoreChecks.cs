namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanSlotStoreChecks
{
    public static void MemorySpanSlotStoreFindsWhatItSaved()
    {
        var store = SpanSlotStore.NewMemorySpanSlotStore();
        var item = SpanSlot.NewSpanSlot("code-431", "label-149");
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
        MemorySpanSlotStoreFindsWhatItSaved();
    }
}
