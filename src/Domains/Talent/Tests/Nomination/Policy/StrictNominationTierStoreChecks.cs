namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class StrictNominationTierStoreChecks
{
    public static void MemoryStrictNominationTierStoreFindsWhatItSaved()
    {
        var store = StrictNominationTierStore.NewMemoryStrictNominationTierStore();
        var item = StrictNominationTier.NewStrictNominationTier("code-302", "title-961");
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
        MemoryStrictNominationTierStoreFindsWhatItSaved();
    }
}
