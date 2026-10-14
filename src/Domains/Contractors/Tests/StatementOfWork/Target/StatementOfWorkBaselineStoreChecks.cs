namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StatementOfWorkBaselineStoreChecks
{
    public static void MemoryStatementOfWorkBaselineStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkBaselineStore.NewMemoryStatementOfWorkBaselineStore();
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("code-45", "label-197");
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
        MemoryStatementOfWorkBaselineStoreFindsWhatItSaved();
    }
}
