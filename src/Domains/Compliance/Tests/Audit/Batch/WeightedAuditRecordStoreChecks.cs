namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedAuditRecordStoreChecks
{
    public static void MemoryWeightedAuditRecordStoreFindsWhatItSaved()
    {
        var store = WeightedAuditRecordStore.NewMemoryWeightedAuditRecordStore();
        var item = WeightedAuditRecord.NewWeightedAuditRecord("code-249", "label-597");
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
        MemoryWeightedAuditRecordStoreFindsWhatItSaved();
    }
}
