namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkProfileStoreChecks
{
    public static void MemoryStatementOfWorkProfileStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkProfileStore.NewMemoryStatementOfWorkProfileStore();
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("code-59", "label-832");
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
        MemoryStatementOfWorkProfileStoreFindsWhatItSaved();
    }
}
