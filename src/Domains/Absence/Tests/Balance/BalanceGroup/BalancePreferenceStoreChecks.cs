namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalancePreferenceStoreChecks
{
    public static void MemoryBalancePreferenceStoreFindsWhatItSaved()
    {
        var store = BalancePreferenceStore.NewMemoryBalancePreferenceStore();
        var item = BalancePreference.NewBalancePreference("code-393", "title-884");
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
        MemoryBalancePreferenceStoreFindsWhatItSaved();
    }
}
