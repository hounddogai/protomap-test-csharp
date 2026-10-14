namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class LenientReorgSlotStoreChecks
{
    public static void MemoryLenientReorgSlotStoreFindsWhatItSaved()
    {
        var store = LenientReorgSlotStore.NewMemoryLenientReorgSlotStore();
        var item = LenientReorgSlot.NewLenientReorgSlot("code-830", "label-30");
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
        MemoryLenientReorgSlotStoreFindsWhatItSaved();
    }
}
