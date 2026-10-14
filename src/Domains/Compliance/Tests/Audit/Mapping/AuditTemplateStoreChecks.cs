namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditTemplateStoreChecks
{
    public static void MemoryAuditTemplateStoreFindsWhatItSaved()
    {
        var store = AuditTemplateStore.NewMemoryAuditTemplateStore();
        var item = AuditTemplate.NewAuditTemplate("code-611", "title-116");
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
        MemoryAuditTemplateStoreFindsWhatItSaved();
    }
}
