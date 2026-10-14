namespace Acme.Workforce.Compliance.Tests.Certification.Claim;

using Acme.Workforce.Compliance.Certification.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedCertificationAuditStoreChecks
{
    public static void MemoryBatchedCertificationAuditStoreFindsWhatItSaved()
    {
        var store = BatchedCertificationAuditStore.NewMemoryBatchedCertificationAuditStore();
        var item = BatchedCertificationAudit.NewBatchedCertificationAudit("code-972", "name-418");
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
        MemoryBatchedCertificationAuditStoreFindsWhatItSaved();
    }
}
