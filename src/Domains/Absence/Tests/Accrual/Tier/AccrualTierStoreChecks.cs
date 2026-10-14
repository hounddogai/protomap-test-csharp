namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualTierStoreChecks
{
    public static void MemoryAccrualTierStoreFindsWhatItSaved()
    {
        var store = AccrualTierStore.NewMemoryAccrualTierStore();
        var item = AccrualTier.NewAccrualTier("code-140", "label-970");
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
        MemoryAccrualTierStoreFindsWhatItSaved();
    }
}
