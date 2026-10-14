namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedReorgSlotStoreChecks
{
    public static void MemoryWeightedReorgSlotStoreFindsWhatItSaved()
    {
        var store = WeightedReorgSlotStore.NewMemoryWeightedReorgSlotStore();
        var item = WeightedReorgSlot.NewWeightedReorgSlot("code-453", "label-659");
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
        MemoryWeightedReorgSlotStoreFindsWhatItSaved();
    }
}
