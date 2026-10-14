namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class StatementOfWorkBatchStoreChecks
{
    public static void MemoryStatementOfWorkBatchStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkBatchStore.NewMemoryStatementOfWorkBatchStore();
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("code-514", "label-820");
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
        MemoryStatementOfWorkBatchStoreFindsWhatItSaved();
    }
}
