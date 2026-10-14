namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationVarianceStoreChecks
{
    public static void MemoryNominationVarianceStoreFindsWhatItSaved()
    {
        var store = NominationVarianceStore.NewMemoryNominationVarianceStore();
        var item = NominationVariance.NewNominationVariance("code-486", "label-583");
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
        MemoryNominationVarianceStoreFindsWhatItSaved();
    }
}
