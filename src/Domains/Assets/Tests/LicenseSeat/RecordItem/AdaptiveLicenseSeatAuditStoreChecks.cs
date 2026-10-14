namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAuditStoreChecks
{
    public static void MemoryAdaptiveLicenseSeatAuditStoreFindsWhatItSaved()
    {
        var store = AdaptiveLicenseSeatAuditStore.NewMemoryAdaptiveLicenseSeatAuditStore();
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("code-627", "label-563");
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
        MemoryAdaptiveLicenseSeatAuditStoreFindsWhatItSaved();
    }
}
