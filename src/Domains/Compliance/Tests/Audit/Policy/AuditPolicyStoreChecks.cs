namespace Acme.Workforce.Compliance.Tests.Audit.Policy;

using Acme.Workforce.Compliance.Audit.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AuditPolicyStoreChecks
{
    public static void MemoryAuditPolicyStoreFindsWhatItSaved()
    {
        var store = AuditPolicyStore.NewMemoryAuditPolicyStore();
        var item = AuditPolicy.NewAuditPolicy("code-190", "name-644");
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
        MemoryAuditPolicyStoreFindsWhatItSaved();
    }
}
