namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FallbackBalanceThresholdStoreChecks
{
    public static void MemoryFallbackBalanceThresholdStoreFindsWhatItSaved()
    {
        var store = FallbackBalanceThresholdStore.NewMemoryFallbackBalanceThresholdStore();
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("code-680", "title-535");
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
        MemoryFallbackBalanceThresholdStoreFindsWhatItSaved();
    }
}
