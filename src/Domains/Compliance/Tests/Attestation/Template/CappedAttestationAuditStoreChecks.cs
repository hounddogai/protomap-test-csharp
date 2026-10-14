namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedAttestationAuditStoreChecks
{
    public static void MemoryCappedAttestationAuditStoreFindsWhatItSaved()
    {
        var store = CappedAttestationAuditStore.NewMemoryCappedAttestationAuditStore();
        var item = CappedAttestationAudit.NewCappedAttestationAudit("code-37", "label-662");
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
        MemoryCappedAttestationAuditStoreFindsWhatItSaved();
    }
}
