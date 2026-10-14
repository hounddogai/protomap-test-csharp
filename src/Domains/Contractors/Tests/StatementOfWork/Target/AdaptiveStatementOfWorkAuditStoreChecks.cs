namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveStatementOfWorkAuditStoreChecks
{
    public static void MemoryAdaptiveStatementOfWorkAuditStoreFindsWhatItSaved()
    {
        var store = AdaptiveStatementOfWorkAuditStore.NewMemoryAdaptiveStatementOfWorkAuditStore();
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("code-259", "label-775");
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
        MemoryAdaptiveStatementOfWorkAuditStoreFindsWhatItSaved();
    }
}
