namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountSlotStoreChecks
{
    public static void MemoryHeadcountSlotStoreFindsWhatItSaved()
    {
        var store = HeadcountSlotStore.NewMemoryHeadcountSlotStore();
        var item = HeadcountSlot.NewHeadcountSlot("code-7", "title-318");
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
        MemoryHeadcountSlotStoreFindsWhatItSaved();
    }
}
