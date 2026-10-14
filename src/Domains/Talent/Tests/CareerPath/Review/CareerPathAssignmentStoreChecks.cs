namespace Acme.Workforce.Talent.Tests.CareerPath.Review;

using Acme.Workforce.Talent.CareerPath.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CareerPathAssignmentStoreChecks
{
    public static void MemoryCareerPathAssignmentStoreFindsWhatItSaved()
    {
        var store = CareerPathAssignmentStore.NewMemoryCareerPathAssignmentStore();
        var item = CareerPathAssignment.NewCareerPathAssignment("code-607", "name-71");
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
        MemoryCareerPathAssignmentStoreFindsWhatItSaved();
    }
}
