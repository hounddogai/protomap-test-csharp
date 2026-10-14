namespace Acme.Workforce.Talent.Tests.Nomination.Assignment;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientNominationRequestStoreChecks
{
    public static void MemoryLenientNominationRequestStoreFindsWhatItSaved()
    {
        var store = LenientNominationRequestStore.NewMemoryLenientNominationRequestStore();
        var item = LenientNominationRequest.NewLenientNominationRequest("code-215", "label-56");
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
        MemoryLenientNominationRequestStoreFindsWhatItSaved();
    }
}
