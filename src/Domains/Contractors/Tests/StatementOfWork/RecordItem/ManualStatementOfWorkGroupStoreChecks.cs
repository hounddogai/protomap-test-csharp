namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ManualStatementOfWorkGroupStoreChecks
{
    public static void MemoryManualStatementOfWorkGroupStoreFindsWhatItSaved()
    {
        var store = ManualStatementOfWorkGroupStore.NewMemoryManualStatementOfWorkGroupStore();
        var item = ManualStatementOfWorkGroup.NewManualStatementOfWorkGroup("code-453", "label-552");
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
        MemoryManualStatementOfWorkGroupStoreFindsWhatItSaved();
    }
}
