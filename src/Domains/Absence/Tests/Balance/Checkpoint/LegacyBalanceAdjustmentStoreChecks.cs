namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyBalanceAdjustmentStoreChecks
{
    public static void MemoryLegacyBalanceAdjustmentStoreFindsWhatItSaved()
    {
        var store = LegacyBalanceAdjustmentStore.NewMemoryLegacyBalanceAdjustmentStore();
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("code-972", "title-629");
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
        MemoryLegacyBalanceAdjustmentStoreFindsWhatItSaved();
    }
}
