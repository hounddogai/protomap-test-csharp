namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceTargetStoreChecks
{
    public static void MemoryBalanceTargetStoreFindsWhatItSaved()
    {
        var store = BalanceTargetStore.NewMemoryBalanceTargetStore();
        var item = BalanceTarget.NewBalanceTarget("code-89", "name-233");
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
        MemoryBalanceTargetStoreFindsWhatItSaved();
    }
}
