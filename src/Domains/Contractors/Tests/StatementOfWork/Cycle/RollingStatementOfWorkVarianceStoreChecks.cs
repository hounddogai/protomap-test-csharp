namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingStatementOfWorkVarianceStoreChecks
{
    public static void MemoryRollingStatementOfWorkVarianceStoreFindsWhatItSaved()
    {
        var store = RollingStatementOfWorkVarianceStore.NewMemoryRollingStatementOfWorkVarianceStore();
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("code-565", "title-301");
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
        MemoryRollingStatementOfWorkVarianceStoreFindsWhatItSaved();
    }
}
