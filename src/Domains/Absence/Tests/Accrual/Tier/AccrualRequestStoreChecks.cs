namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualRequestStoreChecks
{
    public static void MemoryAccrualRequestStoreFindsWhatItSaved()
    {
        var store = AccrualRequestStore.NewMemoryAccrualRequestStore();
        var item = AccrualRequest.NewAccrualRequest("code-753", "name-408");
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
        MemoryAccrualRequestStoreFindsWhatItSaved();
    }
}
