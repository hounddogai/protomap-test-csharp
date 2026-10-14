namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedNominationAssignmentStoreChecks
{
    public static void MemoryCappedNominationAssignmentStoreFindsWhatItSaved()
    {
        var store = CappedNominationAssignmentStore.NewMemoryCappedNominationAssignmentStore();
        var item = CappedNominationAssignment.NewCappedNominationAssignment("code-261", "name-641");
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
        MemoryCappedNominationAssignmentStoreFindsWhatItSaved();
    }
}
