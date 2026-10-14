namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationBundleStoreChecks
{
    public static void MemoryNominationBundleStoreFindsWhatItSaved()
    {
        var store = NominationBundleStore.NewMemoryNominationBundleStore();
        var item = NominationBundle.NewNominationBundle("code-261", "name-279");
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
        MemoryNominationBundleStoreFindsWhatItSaved();
    }
}
