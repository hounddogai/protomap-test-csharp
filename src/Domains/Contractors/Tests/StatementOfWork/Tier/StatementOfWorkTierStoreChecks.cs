namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkTierStoreChecks
{
    public static void MemoryStatementOfWorkTierStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkTierStore.NewMemoryStatementOfWorkTierStore();
        var item = StatementOfWorkTier.NewStatementOfWorkTier("code-871", "label-379");
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
        MemoryStatementOfWorkTierStoreFindsWhatItSaved();
    }
}
