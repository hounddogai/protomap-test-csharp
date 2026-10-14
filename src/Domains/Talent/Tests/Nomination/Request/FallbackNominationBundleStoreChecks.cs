namespace Acme.Workforce.Talent.Tests.Nomination.Request;

using Acme.Workforce.Talent.Nomination.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackNominationBundleStoreChecks
{
    public static void MemoryFallbackNominationBundleStoreFindsWhatItSaved()
    {
        var store = FallbackNominationBundleStore.NewMemoryFallbackNominationBundleStore();
        var item = FallbackNominationBundle.NewFallbackNominationBundle("code-239", "title-29");
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
        MemoryFallbackNominationBundleStoreFindsWhatItSaved();
    }
}
