namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class PrimaryMobilityAuditStoreChecks
{
    public static void MemoryPrimaryMobilityAuditStoreFindsWhatItSaved()
    {
        var store = PrimaryMobilityAuditStore.NewMemoryPrimaryMobilityAuditStore();
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("code-678", "title-217");
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
        MemoryPrimaryMobilityAuditStoreFindsWhatItSaved();
    }
}
