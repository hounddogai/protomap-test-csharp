namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkAllocationStoreChecks
{
    public static void MemoryStatementOfWorkAllocationStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkAllocationStore.NewMemoryStatementOfWorkAllocationStore();
        var item = StatementOfWorkAllocation.NewStatementOfWorkAllocation("code-539", "name-955");
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
        MemoryStatementOfWorkAllocationStoreFindsWhatItSaved();
    }
}
