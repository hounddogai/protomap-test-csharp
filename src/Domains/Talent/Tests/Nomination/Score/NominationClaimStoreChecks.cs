namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class NominationClaimStoreChecks
{
    public static void MemoryNominationClaimStoreFindsWhatItSaved()
    {
        var store = NominationClaimStore.NewMemoryNominationClaimStore();
        var item = NominationClaim.NewNominationClaim("code-256", "title-307");
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
        MemoryNominationClaimStoreFindsWhatItSaved();
    }
}
