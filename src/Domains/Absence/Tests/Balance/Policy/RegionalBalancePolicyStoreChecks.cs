namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalBalancePolicyStoreChecks
{
    public static void MemoryRegionalBalancePolicyStoreFindsWhatItSaved()
    {
        var store = RegionalBalancePolicyStore.NewMemoryRegionalBalancePolicyStore();
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("code-651", "title-480");
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
        MemoryRegionalBalancePolicyStoreFindsWhatItSaved();
    }
}
