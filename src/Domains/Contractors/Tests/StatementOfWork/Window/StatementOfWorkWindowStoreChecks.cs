namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StatementOfWorkWindowStoreChecks
{
    public static void MemoryStatementOfWorkWindowStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkWindowStore.NewMemoryStatementOfWorkWindowStore();
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("code-382", "name-813");
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
        MemoryStatementOfWorkWindowStoreFindsWhatItSaved();
    }
}
