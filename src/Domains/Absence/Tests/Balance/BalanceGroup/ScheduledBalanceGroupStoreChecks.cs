namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class ScheduledBalanceGroupStoreChecks
{
    public static void MemoryScheduledBalanceGroupStoreFindsWhatItSaved()
    {
        var store = ScheduledBalanceGroupStore.NewMemoryScheduledBalanceGroupStore();
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("code-125", "name-198");
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
        MemoryScheduledBalanceGroupStoreFindsWhatItSaved();
    }
}
