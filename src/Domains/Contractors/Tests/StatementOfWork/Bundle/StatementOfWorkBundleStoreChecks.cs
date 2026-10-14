namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkBundleStoreChecks
{
    public static void MemoryStatementOfWorkBundleStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkBundleStore.NewMemoryStatementOfWorkBundleStore();
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("code-551", "name-155");
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
        MemoryStatementOfWorkBundleStoreFindsWhatItSaved();
    }
}
