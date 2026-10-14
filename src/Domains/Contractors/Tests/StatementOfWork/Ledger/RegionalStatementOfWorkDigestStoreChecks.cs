namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkDigestStoreChecks
{
    public static void MemoryRegionalStatementOfWorkDigestStoreFindsWhatItSaved()
    {
        var store = RegionalStatementOfWorkDigestStore.NewMemoryRegionalStatementOfWorkDigestStore();
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("code-56", "name-361");
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
        MemoryRegionalStatementOfWorkDigestStoreFindsWhatItSaved();
    }
}
