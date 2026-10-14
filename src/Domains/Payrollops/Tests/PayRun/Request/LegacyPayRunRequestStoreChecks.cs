namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyPayRunRequestStoreChecks
{
    public static void MemoryLegacyPayRunRequestStoreFindsWhatItSaved()
    {
        var store = LegacyPayRunRequestStore.NewMemoryLegacyPayRunRequestStore();
        var item = LegacyPayRunRequest.NewLegacyPayRunRequest("code-564", "title-577");
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
        MemoryLegacyPayRunRequestStoreFindsWhatItSaved();
    }
}
