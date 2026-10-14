namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementAuditStoreChecks
{
    public static void MemoryEngagementAuditStoreFindsWhatItSaved()
    {
        var store = EngagementAuditStore.NewMemoryEngagementAuditStore();
        var item = EngagementAudit.NewEngagementAudit("code-396", "name-419");
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
        MemoryEngagementAuditStoreFindsWhatItSaved();
    }
}
