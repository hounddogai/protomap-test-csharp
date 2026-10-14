namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgAdjustmentStoreChecks
{
    public static void MemoryReorgAdjustmentStoreFindsWhatItSaved()
    {
        var store = ReorgAdjustmentStore.NewMemoryReorgAdjustmentStore();
        var item = ReorgAdjustment.NewReorgAdjustment("code-810", "label-575");
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
        MemoryReorgAdjustmentStoreFindsWhatItSaved();
    }
}
