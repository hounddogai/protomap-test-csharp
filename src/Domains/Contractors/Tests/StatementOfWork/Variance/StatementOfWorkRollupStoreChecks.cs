namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkRollupStoreChecks
{
    public static void MemoryStatementOfWorkRollupStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkRollupStore.NewMemoryStatementOfWorkRollupStore();
        var item = StatementOfWorkRollup.NewStatementOfWorkRollup("code-380", "label-820");
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
        MemoryStatementOfWorkRollupStoreFindsWhatItSaved();
    }
}
