namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class MobilityAuditStoreChecks
{
    public static void MemoryMobilityAuditStoreFindsWhatItSaved()
    {
        var store = MobilityAuditStore.NewMemoryMobilityAuditStore();
        var item = MobilityAudit.NewMobilityAudit("code-253", "title-965");
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
        MemoryMobilityAuditStoreFindsWhatItSaved();
    }
}
