namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CertificationQuotaStoreChecks
{
    public static void MemoryCertificationQuotaStoreFindsWhatItSaved()
    {
        var store = CertificationQuotaStore.NewMemoryCertificationQuotaStore();
        var item = CertificationQuota.NewCertificationQuota("code-204", "title-251");
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
        MemoryCertificationQuotaStoreFindsWhatItSaved();
    }
}
