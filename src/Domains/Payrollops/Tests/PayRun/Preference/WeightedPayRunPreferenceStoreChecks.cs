namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedPayRunPreferenceStoreChecks
{
    public static void MemoryWeightedPayRunPreferenceStoreFindsWhatItSaved()
    {
        var store = WeightedPayRunPreferenceStore.NewMemoryWeightedPayRunPreferenceStore();
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("code-676", "title-466");
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
        MemoryWeightedPayRunPreferenceStoreFindsWhatItSaved();
    }
}
