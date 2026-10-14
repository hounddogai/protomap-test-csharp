namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkAssignmentStoreChecks
{
    public static void MemoryStatementOfWorkAssignmentStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkAssignmentStore.NewMemoryStatementOfWorkAssignmentStore();
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("code-748", "name-75");
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
        MemoryStatementOfWorkAssignmentStoreFindsWhatItSaved();
    }
}
