namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceVarianceStoreChecks
{
    public static void MemoryBalanceVarianceStoreFindsWhatItSaved()
    {
        var store = BalanceVarianceStore.NewMemoryBalanceVarianceStore();
        var item = BalanceVariance.NewBalanceVariance("code-848", "name-2");
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
        MemoryBalanceVarianceStoreFindsWhatItSaved();
    }
}
