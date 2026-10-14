namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientStatementOfWorkDigestStoreChecks
{
    public static void MemoryLenientStatementOfWorkDigestStoreFindsWhatItSaved()
    {
        var store = LenientStatementOfWorkDigestStore.NewMemoryLenientStatementOfWorkDigestStore();
        var item = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest("code-323", "title-975");
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
        MemoryLenientStatementOfWorkDigestStoreFindsWhatItSaved();
    }
}
