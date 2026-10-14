namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedAuditPlanStoreChecks
{
    public static void MemoryCappedAuditPlanStoreFindsWhatItSaved()
    {
        var store = CappedAuditPlanStore.NewMemoryCappedAuditPlanStore();
        var item = CappedAuditPlan.NewCappedAuditPlan("code-43", "label-39");
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
        MemoryCappedAuditPlanStoreFindsWhatItSaved();
    }
}
