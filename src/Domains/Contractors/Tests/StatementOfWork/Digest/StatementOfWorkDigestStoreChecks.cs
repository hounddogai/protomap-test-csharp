namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkDigestStoreChecks
{
    public static void MemoryStatementOfWorkDigestStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkDigestStore.NewMemoryStatementOfWorkDigestStore();
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("code-891", "name-450");
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
        MemoryStatementOfWorkDigestStoreFindsWhatItSaved();
    }
}
