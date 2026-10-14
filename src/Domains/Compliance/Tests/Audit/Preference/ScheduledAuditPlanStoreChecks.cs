namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledAuditPlanStoreChecks
{
    public static void MemoryScheduledAuditPlanStoreFindsWhatItSaved()
    {
        var store = ScheduledAuditPlanStore.NewMemoryScheduledAuditPlanStore();
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("code-823", "name-11");
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
        MemoryScheduledAuditPlanStoreFindsWhatItSaved();
    }
}
