namespace Acme.Workforce.Compliance.Tests.Audit.Draft;

using Acme.Workforce.Compliance.Audit.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AuditRequestStoreChecks
{
    public static void MemoryAuditRequestStoreFindsWhatItSaved()
    {
        var store = AuditRequestStore.NewMemoryAuditRequestStore();
        var item = AuditRequest.NewAuditRequest("code-870", "label-798");
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
        MemoryAuditRequestStoreFindsWhatItSaved();
    }
}
