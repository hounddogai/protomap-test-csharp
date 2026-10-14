namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CertificationBundleStoreChecks
{
    public static void MemoryCertificationBundleStoreFindsWhatItSaved()
    {
        var store = CertificationBundleStore.NewMemoryCertificationBundleStore();
        var item = CertificationBundle.NewCertificationBundle("code-348", "label-340");
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
        MemoryCertificationBundleStoreFindsWhatItSaved();
    }
}
