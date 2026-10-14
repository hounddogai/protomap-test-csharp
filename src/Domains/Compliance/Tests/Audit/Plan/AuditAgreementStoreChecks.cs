namespace Acme.Workforce.Compliance.Tests.Audit.Plan;

using Acme.Workforce.Compliance.Audit.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AuditAgreementStoreChecks
{
    public static void MemoryAuditAgreementStoreFindsWhatItSaved()
    {
        var store = AuditAgreementStore.NewMemoryAuditAgreementStore();
        var item = AuditAgreement.NewAuditAgreement("code-139", "name-677");
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
        MemoryAuditAgreementStoreFindsWhatItSaved();
    }
}
