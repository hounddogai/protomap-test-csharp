namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientStatementOfWorkCheckpointStoreChecks
{
    public static void MemoryLenientStatementOfWorkCheckpointStoreFindsWhatItSaved()
    {
        var store = LenientStatementOfWorkCheckpointStore.NewMemoryLenientStatementOfWorkCheckpointStore();
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("code-495", "name-934");
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
        MemoryLenientStatementOfWorkCheckpointStoreFindsWhatItSaved();
    }
}
