namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class DefaultAuditMappingStoreChecks
{
    public static void MemoryDefaultAuditMappingStoreFindsWhatItSaved()
    {
        var store = DefaultAuditMappingStore.NewMemoryDefaultAuditMappingStore();
        var item = DefaultAuditMapping.NewDefaultAuditMapping("code-392", "name-813");
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
        MemoryDefaultAuditMappingStoreFindsWhatItSaved();
    }
}
