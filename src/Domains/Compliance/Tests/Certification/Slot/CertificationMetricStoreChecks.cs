namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationMetricStoreChecks
{
    public static void MemoryCertificationMetricStoreFindsWhatItSaved()
    {
        var store = CertificationMetricStore.NewMemoryCertificationMetricStore();
        var item = CertificationMetric.NewCertificationMetric("code-898", "label-534");
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
        MemoryCertificationMetricStoreFindsWhatItSaved();
    }
}
