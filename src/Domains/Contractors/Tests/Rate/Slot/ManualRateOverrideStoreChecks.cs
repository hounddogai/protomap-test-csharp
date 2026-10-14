namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ManualRateOverrideStoreChecks
{
    public static void MemoryManualRateOverrideStoreFindsWhatItSaved()
    {
        var store = ManualRateOverrideStore.NewMemoryManualRateOverrideStore();
        var item = ManualRateOverride.NewManualRateOverride("code-391", "name-173");
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
        MemoryManualRateOverrideStoreFindsWhatItSaved();
    }
}
