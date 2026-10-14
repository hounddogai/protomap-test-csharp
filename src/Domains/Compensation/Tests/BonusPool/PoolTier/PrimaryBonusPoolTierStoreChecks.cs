namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class PrimaryBonusPoolTierStoreChecks
{
    public static void MemoryPrimaryBonusPoolTierStoreFindsWhatItSaved()
    {
        var store = PrimaryBonusPoolTierStore.NewMemoryPrimaryBonusPoolTierStore();
        var item = PrimaryBonusPoolTier.NewPrimaryBonusPoolTier("code-572", "title-366");
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
        MemoryPrimaryBonusPoolTierStoreFindsWhatItSaved();
    }
}
