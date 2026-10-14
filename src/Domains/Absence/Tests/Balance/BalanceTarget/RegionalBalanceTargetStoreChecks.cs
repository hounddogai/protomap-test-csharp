namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class RegionalBalanceTargetStoreChecks
{
    public static void MemoryRegionalBalanceTargetStoreFindsWhatItSaved()
    {
        var store = RegionalBalanceTargetStore.NewMemoryRegionalBalanceTargetStore();
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("code-502", "name-176");
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
        MemoryRegionalBalanceTargetStoreFindsWhatItSaved();
    }
}
