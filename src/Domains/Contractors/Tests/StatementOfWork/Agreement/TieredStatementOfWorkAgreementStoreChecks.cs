namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredStatementOfWorkAgreementStoreChecks
{
    public static void MemoryTieredStatementOfWorkAgreementStoreFindsWhatItSaved()
    {
        var store = TieredStatementOfWorkAgreementStore.NewMemoryTieredStatementOfWorkAgreementStore();
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("code-770", "name-554");
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
        MemoryTieredStatementOfWorkAgreementStoreFindsWhatItSaved();
    }
}
