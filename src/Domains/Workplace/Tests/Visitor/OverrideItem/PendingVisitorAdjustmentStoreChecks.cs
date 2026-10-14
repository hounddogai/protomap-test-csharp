namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PendingVisitorAdjustmentStoreChecks
{
    public static void MemoryPendingVisitorAdjustmentStoreFindsWhatItSaved()
    {
        var store = PendingVisitorAdjustmentStore.NewMemoryPendingVisitorAdjustmentStore();
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("code-705", "title-869");
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
        MemoryPendingVisitorAdjustmentStoreFindsWhatItSaved();
    }
}
