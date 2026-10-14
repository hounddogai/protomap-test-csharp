namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolTierStoreChecks
{
    public static void MemoryPoolTierStoreFindsWhatItSaved()
    {
        var store = PoolTierStore.NewMemoryPoolTierStore();
        var item = PoolTier.NewPoolTier("code-931", "title-742");
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
        MemoryPoolTierStoreFindsWhatItSaved();
    }
}
