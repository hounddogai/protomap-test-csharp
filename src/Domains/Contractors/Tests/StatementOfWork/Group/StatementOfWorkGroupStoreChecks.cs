namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Group;

using Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class StatementOfWorkGroupStoreChecks
{
    public static void MemoryStatementOfWorkGroupStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkGroupStore.NewMemoryStatementOfWorkGroupStore();
        var item = StatementOfWorkGroup.NewStatementOfWorkGroup("code-226", "label-243");
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
        MemoryStatementOfWorkGroupStoreFindsWhatItSaved();
    }
}
