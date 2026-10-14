namespace Acme.Workforce.Talent.Tests.Nomination.Target;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class NominationTargetStoreChecks
{
    public static void MemoryNominationTargetStoreFindsWhatItSaved()
    {
        var store = NominationTargetStore.NewMemoryNominationTargetStore();
        var item = NominationTarget.NewNominationTarget("code-512", "name-19");
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
        MemoryNominationTargetStoreFindsWhatItSaved();
    }
}
