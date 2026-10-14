namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientAuditPlanStoreChecks
{
    public static void MemoryLenientAuditPlanStoreFindsWhatItSaved()
    {
        var store = LenientAuditPlanStore.NewMemoryLenientAuditPlanStore();
        var item = LenientAuditPlan.NewLenientAuditPlan("code-474", "label-825");
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
        MemoryLenientAuditPlanStoreFindsWhatItSaved();
    }
}
