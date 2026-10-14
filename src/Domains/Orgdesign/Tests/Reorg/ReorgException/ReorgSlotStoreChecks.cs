namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgSlotStoreChecks
{
    public static void MemoryReorgSlotStoreFindsWhatItSaved()
    {
        var store = ReorgSlotStore.NewMemoryReorgSlotStore();
        var item = ReorgSlot.NewReorgSlot("code-576", "title-973");
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
        MemoryReorgSlotStoreFindsWhatItSaved();
    }
}
