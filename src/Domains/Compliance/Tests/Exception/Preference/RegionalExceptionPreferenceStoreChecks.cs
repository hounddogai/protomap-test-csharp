namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalExceptionPreferenceStoreChecks
{
    public static void MemoryRegionalExceptionPreferenceStoreFindsWhatItSaved()
    {
        var store = RegionalExceptionPreferenceStore.NewMemoryRegionalExceptionPreferenceStore();
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("code-623", "label-904");
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
        MemoryRegionalExceptionPreferenceStoreFindsWhatItSaved();
    }
}
