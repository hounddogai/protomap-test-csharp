namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkCheckpointStoreChecks
{
    public static void MemoryStatementOfWorkCheckpointStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkCheckpointStore.NewMemoryStatementOfWorkCheckpointStore();
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("code-9", "title-42");
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
        MemoryStatementOfWorkCheckpointStoreFindsWhatItSaved();
    }
}
