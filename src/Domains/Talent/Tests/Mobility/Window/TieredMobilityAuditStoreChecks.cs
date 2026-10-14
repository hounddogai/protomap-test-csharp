namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredMobilityAuditStoreChecks
{
    public static void MemoryTieredMobilityAuditStoreFindsWhatItSaved()
    {
        var store = TieredMobilityAuditStore.NewMemoryTieredMobilityAuditStore();
        var item = TieredMobilityAudit.NewTieredMobilityAudit("code-772", "name-766");
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
        MemoryTieredMobilityAuditStoreFindsWhatItSaved();
    }
}
