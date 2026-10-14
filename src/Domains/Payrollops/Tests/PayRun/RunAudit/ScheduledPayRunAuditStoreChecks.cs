namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class ScheduledPayRunAuditStoreChecks
{
    public static void MemoryScheduledPayRunAuditStoreFindsWhatItSaved()
    {
        var store = ScheduledPayRunAuditStore.NewMemoryScheduledPayRunAuditStore();
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("code-140", "label-979");
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
        MemoryScheduledPayRunAuditStoreFindsWhatItSaved();
    }
}
