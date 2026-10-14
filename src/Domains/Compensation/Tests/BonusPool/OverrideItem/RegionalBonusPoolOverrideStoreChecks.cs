namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalBonusPoolOverrideStoreChecks
{
    public static void MemoryRegionalBonusPoolOverrideStoreFindsWhatItSaved()
    {
        var store = RegionalBonusPoolOverrideStore.NewMemoryRegionalBonusPoolOverrideStore();
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("code-971", "label-9");
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
        MemoryRegionalBonusPoolOverrideStoreFindsWhatItSaved();
    }
}
