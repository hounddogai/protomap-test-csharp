namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class SeasonalAuditAssignmentStoreChecks
{
    public static void MemorySeasonalAuditAssignmentStoreFindsWhatItSaved()
    {
        var store = SeasonalAuditAssignmentStore.NewMemorySeasonalAuditAssignmentStore();
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("code-925", "name-265");
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
        MemorySeasonalAuditAssignmentStoreFindsWhatItSaved();
    }
}
