namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayPreferenceStoreChecks
{
    public static void MemoryLenientRetroPayPreferenceStoreFindsWhatItSaved()
    {
        var store = LenientRetroPayPreferenceStore.NewMemoryLenientRetroPayPreferenceStore();
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("code-758", "title-788");
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
        MemoryLenientRetroPayPreferenceStoreFindsWhatItSaved();
    }
}
