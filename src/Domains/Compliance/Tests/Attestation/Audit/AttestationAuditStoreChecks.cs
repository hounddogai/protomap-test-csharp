namespace Acme.Workforce.Compliance.Tests.Attestation.Audit;

using Acme.Workforce.Compliance.Attestation.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AttestationAuditStoreChecks
{
    public static void MemoryAttestationAuditStoreFindsWhatItSaved()
    {
        var store = AttestationAuditStore.NewMemoryAttestationAuditStore();
        var item = AttestationAudit.NewAttestationAudit("code-445", "title-29");
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
        MemoryAttestationAuditStoreFindsWhatItSaved();
    }
}
