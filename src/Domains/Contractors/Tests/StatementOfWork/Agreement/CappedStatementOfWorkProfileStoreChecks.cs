namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProfileStoreChecks
{
    public static void MemoryCappedStatementOfWorkProfileStoreFindsWhatItSaved()
    {
        var store = CappedStatementOfWorkProfileStore.NewMemoryCappedStatementOfWorkProfileStore();
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("code-988", "title-387");
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
        MemoryCappedStatementOfWorkProfileStoreFindsWhatItSaved();
    }
}
