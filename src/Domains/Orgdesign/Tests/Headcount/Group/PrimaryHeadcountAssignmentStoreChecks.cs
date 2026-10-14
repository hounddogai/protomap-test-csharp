namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryHeadcountAssignmentStoreChecks
{
    public static void MemoryPrimaryHeadcountAssignmentStoreFindsWhatItSaved()
    {
        var store = PrimaryHeadcountAssignmentStore.NewMemoryPrimaryHeadcountAssignmentStore();
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("code-728", "title-289");
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
        MemoryPrimaryHeadcountAssignmentStoreFindsWhatItSaved();
    }
}
