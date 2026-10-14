namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalNominationVarianceStoreChecks
{
    public static void MemoryRegionalNominationVarianceStoreFindsWhatItSaved()
    {
        var store = RegionalNominationVarianceStore.NewMemoryRegionalNominationVarianceStore();
        var item = RegionalNominationVariance.NewRegionalNominationVariance("code-915", "name-703");
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
        MemoryRegionalNominationVarianceStoreFindsWhatItSaved();
    }
}
