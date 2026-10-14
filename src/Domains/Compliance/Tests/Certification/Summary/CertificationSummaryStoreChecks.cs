namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationSummaryStoreChecks
{
    public static void MemoryCertificationSummaryStoreFindsWhatItSaved()
    {
        var store = CertificationSummaryStore.NewMemoryCertificationSummaryStore();
        var item = CertificationSummary.NewCertificationSummary("code-1", "label-362");
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
        MemoryCertificationSummaryStoreFindsWhatItSaved();
    }
}
