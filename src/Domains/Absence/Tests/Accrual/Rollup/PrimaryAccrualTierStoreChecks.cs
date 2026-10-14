namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryAccrualTierStoreChecks
{
    public static void MemoryPrimaryAccrualTierStoreFindsWhatItSaved()
    {
        var store = PrimaryAccrualTierStore.NewMemoryPrimaryAccrualTierStore();
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("code-770", "name-268");
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
        MemoryPrimaryAccrualTierStoreFindsWhatItSaved();
    }
}
