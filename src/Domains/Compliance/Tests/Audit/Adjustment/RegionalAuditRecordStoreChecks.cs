namespace Acme.Workforce.Compliance.Tests.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RegionalAuditRecordStoreChecks
{
    public static void MemoryRegionalAuditRecordStoreFindsWhatItSaved()
    {
        var store = RegionalAuditRecordStore.NewMemoryRegionalAuditRecordStore();
        var item = RegionalAuditRecord.NewRegionalAuditRecord("code-303", "label-653");
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
        MemoryRegionalAuditRecordStoreFindsWhatItSaved();
    }
}
