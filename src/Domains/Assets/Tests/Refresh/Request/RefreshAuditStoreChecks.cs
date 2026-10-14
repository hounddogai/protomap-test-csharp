namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RefreshAuditStoreChecks
{
    public static void MemoryRefreshAuditStoreFindsWhatItSaved()
    {
        var store = RefreshAuditStore.NewMemoryRefreshAuditStore();
        var item = RefreshAudit.NewRefreshAudit("code-587", "title-538");
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
        MemoryRefreshAuditStoreFindsWhatItSaved();
    }
}
