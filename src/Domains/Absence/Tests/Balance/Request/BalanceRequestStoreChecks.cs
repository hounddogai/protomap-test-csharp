namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BalanceRequestStoreChecks
{
    public static void MemoryBalanceRequestStoreFindsWhatItSaved()
    {
        var store = BalanceRequestStore.NewMemoryBalanceRequestStore();
        var item = BalanceRequest.NewBalanceRequest("code-970", "name-361");
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
        MemoryBalanceRequestStoreFindsWhatItSaved();
    }
}
