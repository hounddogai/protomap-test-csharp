namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientStatementOfWorkBundleStoreChecks
{
    public static void MemoryLenientStatementOfWorkBundleStoreFindsWhatItSaved()
    {
        var store = LenientStatementOfWorkBundleStore.NewMemoryLenientStatementOfWorkBundleStore();
        var item = LenientStatementOfWorkBundle.NewLenientStatementOfWorkBundle("code-761", "name-201");
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
        MemoryLenientStatementOfWorkBundleStoreFindsWhatItSaved();
    }
}
