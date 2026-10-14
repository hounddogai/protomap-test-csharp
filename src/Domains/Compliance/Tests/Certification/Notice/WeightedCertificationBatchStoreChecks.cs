namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedCertificationBatchStoreChecks
{
    public static void MemoryWeightedCertificationBatchStoreFindsWhatItSaved()
    {
        var store = WeightedCertificationBatchStore.NewMemoryWeightedCertificationBatchStore();
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("code-963", "title-448");
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
        MemoryWeightedCertificationBatchStoreFindsWhatItSaved();
    }
}
