namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkRecordStoreChecks
{
    public static void MemoryStatementOfWorkRecordStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkRecordStore.NewMemoryStatementOfWorkRecordStore();
        var item = StatementOfWorkRecord.NewStatementOfWorkRecord("code-4", "name-63");
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
        MemoryStatementOfWorkRecordStoreFindsWhatItSaved();
    }
}
