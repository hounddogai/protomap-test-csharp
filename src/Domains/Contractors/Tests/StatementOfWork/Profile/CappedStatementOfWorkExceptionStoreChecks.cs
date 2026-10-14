namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Profile;

using Acme.Workforce.Contractors.StatementOfWork.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedStatementOfWorkExceptionStoreChecks
{
    public static void MemoryCappedStatementOfWorkExceptionStoreFindsWhatItSaved()
    {
        var store = CappedStatementOfWorkExceptionStore.NewMemoryCappedStatementOfWorkExceptionStore();
        var item = CappedStatementOfWorkException.NewCappedStatementOfWorkException("code-789", "name-124");
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
        MemoryCappedStatementOfWorkExceptionStoreFindsWhatItSaved();
    }
}
