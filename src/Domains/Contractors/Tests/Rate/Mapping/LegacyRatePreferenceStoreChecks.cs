namespace Acme.Workforce.Contractors.Tests.Rate.Mapping;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyRatePreferenceStoreChecks
{
    public static void MemoryLegacyRatePreferenceStoreFindsWhatItSaved()
    {
        var store = LegacyRatePreferenceStore.NewMemoryLegacyRatePreferenceStore();
        var item = LegacyRatePreference.NewLegacyRatePreference("code-596", "title-600");
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
        MemoryLegacyRatePreferenceStoreFindsWhatItSaved();
    }
}
