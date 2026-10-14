namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class TieredCertificationSummaryStoreChecks
{
    public static void MemoryTieredCertificationSummaryStoreFindsWhatItSaved()
    {
        var store = TieredCertificationSummaryStore.NewMemoryTieredCertificationSummaryStore();
        var item = TieredCertificationSummary.NewTieredCertificationSummary("code-471", "title-598");
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
        MemoryTieredCertificationSummaryStoreFindsWhatItSaved();
    }
}
