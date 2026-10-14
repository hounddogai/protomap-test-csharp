namespace Acme.Workforce.Compliance.Tests.Audit.Slot;

using Acme.Workforce.Compliance.Audit.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AuditSlotStoreChecks
{
    public static void MemoryAuditSlotStoreFindsWhatItSaved()
    {
        var store = AuditSlotStore.NewMemoryAuditSlotStore();
        var item = AuditSlot.NewAuditSlot("code-469", "title-825");
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
        MemoryAuditSlotStoreFindsWhatItSaved();
    }
}
