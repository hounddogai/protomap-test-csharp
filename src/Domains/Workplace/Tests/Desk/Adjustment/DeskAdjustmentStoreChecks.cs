namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskAdjustmentStoreChecks
{
    public static void MemoryDeskAdjustmentStoreFindsWhatItSaved()
    {
        var store = DeskAdjustmentStore.NewMemoryDeskAdjustmentStore();
        var item = DeskAdjustment.NewDeskAdjustment("code-467", "title-345");
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
        MemoryDeskAdjustmentStoreFindsWhatItSaved();
    }
}
