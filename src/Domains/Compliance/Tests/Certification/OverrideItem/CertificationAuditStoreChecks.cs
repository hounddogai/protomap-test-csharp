namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationAuditStoreChecks
{
    public static void MemoryCertificationAuditStoreFindsWhatItSaved()
    {
        var store = CertificationAuditStore.NewMemoryCertificationAuditStore();
        var item = CertificationAudit.NewCertificationAudit("code-260", "label-397");
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
        MemoryCertificationAuditStoreFindsWhatItSaved();
    }
}
