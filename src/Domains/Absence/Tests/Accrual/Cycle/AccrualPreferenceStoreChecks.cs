namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualPreferenceStoreChecks
{
    public static void MemoryAccrualPreferenceStoreFindsWhatItSaved()
    {
        var store = AccrualPreferenceStore.NewMemoryAccrualPreferenceStore();
        var item = AccrualPreference.NewAccrualPreference("code-785", "title-197");
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
        MemoryAccrualPreferenceStoreFindsWhatItSaved();
    }
}
