namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictNominationProfileStoreChecks
{
    public static void MemoryStrictNominationProfileStoreFindsWhatItSaved()
    {
        var store = StrictNominationProfileStore.NewMemoryStrictNominationProfileStore();
        var item = StrictNominationProfile.NewStrictNominationProfile("code-486", "name-627");
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
        MemoryStrictNominationProfileStoreFindsWhatItSaved();
    }
}
