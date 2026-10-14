namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkAdjustmentStoreChecks
{
    public static void MemoryStatementOfWorkAdjustmentStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkAdjustmentStore.NewMemoryStatementOfWorkAdjustmentStore();
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("code-134", "title-646");
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
        MemoryStatementOfWorkAdjustmentStoreFindsWhatItSaved();
    }
}
