namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveNominationVarianceStoreChecks
{
    public static void MemoryAdaptiveNominationVarianceStoreFindsWhatItSaved()
    {
        var store = AdaptiveNominationVarianceStore.NewMemoryAdaptiveNominationVarianceStore();
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("code-591", "label-574");
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
        MemoryAdaptiveNominationVarianceStoreFindsWhatItSaved();
    }
}
