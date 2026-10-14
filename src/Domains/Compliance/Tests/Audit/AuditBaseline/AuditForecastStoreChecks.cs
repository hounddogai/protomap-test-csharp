namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class AuditForecastStoreChecks
{
    public static void MemoryAuditForecastStoreFindsWhatItSaved()
    {
        var store = AuditForecastStore.NewMemoryAuditForecastStore();
        var item = AuditForecast.NewAuditForecast("code-114", "name-134");
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
        MemoryAuditForecastStoreFindsWhatItSaved();
    }
}
