namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkAuditStoreChecks
{
    public static void MemoryStatementOfWorkAuditStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkAuditStore.NewMemoryStatementOfWorkAuditStore();
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("code-506", "name-442");
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
        MemoryStatementOfWorkAuditStoreFindsWhatItSaved();
    }
}
