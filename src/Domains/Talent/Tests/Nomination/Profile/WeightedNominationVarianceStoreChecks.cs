namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedNominationVarianceStoreChecks
{
    public static void MemoryWeightedNominationVarianceStoreFindsWhatItSaved()
    {
        var store = WeightedNominationVarianceStore.NewMemoryWeightedNominationVarianceStore();
        var item = WeightedNominationVariance.NewWeightedNominationVariance("code-706", "title-245");
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
        MemoryWeightedNominationVarianceStoreFindsWhatItSaved();
    }
}
