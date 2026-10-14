namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class WeightedRatePreferenceStoreChecks
{
    public static void MemoryWeightedRatePreferenceStoreFindsWhatItSaved()
    {
        var store = WeightedRatePreferenceStore.NewMemoryWeightedRatePreferenceStore();
        var item = WeightedRatePreference.NewWeightedRatePreference("code-521", "title-820");
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
        MemoryWeightedRatePreferenceStoreFindsWhatItSaved();
    }
}
