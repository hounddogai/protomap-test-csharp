namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class BonusPoolOverrideStoreChecks
{
    public static void MemoryBonusPoolOverrideStoreFindsWhatItSaved()
    {
        var store = BonusPoolOverrideStore.NewMemoryBonusPoolOverrideStore();
        var item = BonusPoolOverride.NewBonusPoolOverride("code-458", "label-172");
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
        MemoryBonusPoolOverrideStoreFindsWhatItSaved();
    }
}
