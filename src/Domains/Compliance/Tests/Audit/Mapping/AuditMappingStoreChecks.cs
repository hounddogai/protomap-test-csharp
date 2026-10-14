namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditMappingStoreChecks
{
    public static void MemoryAuditMappingStoreFindsWhatItSaved()
    {
        var store = AuditMappingStore.NewMemoryAuditMappingStore();
        var item = AuditMapping.NewAuditMapping("code-918", "name-9");
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
        MemoryAuditMappingStoreFindsWhatItSaved();
    }
}
