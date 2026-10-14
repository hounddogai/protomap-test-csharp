namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationAssignmentStoreChecks
{
    public static void MemoryNominationAssignmentStoreFindsWhatItSaved()
    {
        var store = NominationAssignmentStore.NewMemoryNominationAssignmentStore();
        var item = NominationAssignment.NewNominationAssignment("code-441", "label-121");
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
        MemoryNominationAssignmentStoreFindsWhatItSaved();
    }
}
