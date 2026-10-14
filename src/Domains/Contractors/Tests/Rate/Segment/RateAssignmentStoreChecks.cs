namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateAssignmentStoreChecks
{
    public static void MemoryRateAssignmentStoreFindsWhatItSaved()
    {
        var store = RateAssignmentStore.NewMemoryRateAssignmentStore();
        var item = RateAssignment.NewRateAssignment("code-541", "title-170");
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
        MemoryRateAssignmentStoreFindsWhatItSaved();
    }
}
