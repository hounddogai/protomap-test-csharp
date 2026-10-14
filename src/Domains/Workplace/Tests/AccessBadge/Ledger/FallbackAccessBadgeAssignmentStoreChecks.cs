namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackAccessBadgeAssignmentStoreChecks
{
    public static void MemoryFallbackAccessBadgeAssignmentStoreFindsWhatItSaved()
    {
        var store = FallbackAccessBadgeAssignmentStore.NewMemoryFallbackAccessBadgeAssignmentStore();
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("code-884", "name-239");
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
        MemoryFallbackAccessBadgeAssignmentStoreFindsWhatItSaved();
    }
}
