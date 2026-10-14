namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceGroupStoreChecks
{
    public static void MemoryLenientBalanceGroupStoreFindsWhatItSaved()
    {
        var store = LenientBalanceGroupStore.NewMemoryLenientBalanceGroupStore();
        var item = LenientBalanceGroup.NewLenientBalanceGroup("code-263", "name-803");
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
        MemoryLenientBalanceGroupStoreFindsWhatItSaved();
    }
}
