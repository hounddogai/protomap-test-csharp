namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanAuditStoreChecks
{
    public static void MemoryFloorPlanAuditStoreFindsWhatItSaved()
    {
        var store = FloorPlanAuditStore.NewMemoryFloorPlanAuditStore();
        var item = FloorPlanAudit.NewFloorPlanAudit("code-510", "title-824");
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
        MemoryFloorPlanAuditStoreFindsWhatItSaved();
    }
}
