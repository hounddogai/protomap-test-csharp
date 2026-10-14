namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkQuotaStoreChecks
{
    public static void MemoryStatementOfWorkQuotaStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkQuotaStore.NewMemoryStatementOfWorkQuotaStore();
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("code-276", "label-356");
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
        MemoryStatementOfWorkQuotaStoreFindsWhatItSaved();
    }
}
