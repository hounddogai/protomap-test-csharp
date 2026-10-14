namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceGroupStoreChecks
{
    public static void MemoryBalanceGroupStoreFindsWhatItSaved()
    {
        var store = BalanceGroupStore.NewMemoryBalanceGroupStore();
        var item = BalanceGroup.NewBalanceGroup("code-233", "label-372");
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
        MemoryBalanceGroupStoreFindsWhatItSaved();
    }
}
