namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalSpanSlotStoreChecks
{
    public static void MemoryGlobalSpanSlotStoreFindsWhatItSaved()
    {
        var store = GlobalSpanSlotStore.NewMemoryGlobalSpanSlotStore();
        var item = GlobalSpanSlot.NewGlobalSpanSlot("code-324", "title-867");
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
        MemoryGlobalSpanSlotStoreFindsWhatItSaved();
    }
}
