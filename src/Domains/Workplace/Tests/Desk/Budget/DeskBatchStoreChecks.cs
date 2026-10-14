namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeskBatchStoreChecks
{
    public static void MemoryDeskBatchStoreFindsWhatItSaved()
    {
        var store = DeskBatchStore.NewMemoryDeskBatchStore();
        var item = DeskBatch.NewDeskBatch("code-179", "name-378");
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
        MemoryDeskBatchStoreFindsWhatItSaved();
    }
}
