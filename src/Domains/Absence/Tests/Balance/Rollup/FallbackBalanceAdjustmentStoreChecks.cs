namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackBalanceAdjustmentStoreChecks
{
    public static void MemoryFallbackBalanceAdjustmentStoreFindsWhatItSaved()
    {
        var store = FallbackBalanceAdjustmentStore.NewMemoryFallbackBalanceAdjustmentStore();
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("code-901", "label-303");
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
        MemoryFallbackBalanceAdjustmentStoreFindsWhatItSaved();
    }
}
