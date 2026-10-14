namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkRecordStoreChecks
{
    public static void MemoryScheduledStatementOfWorkRecordStoreFindsWhatItSaved()
    {
        var store = ScheduledStatementOfWorkRecordStore.NewMemoryScheduledStatementOfWorkRecordStore();
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("code-240", "name-371");
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
        MemoryScheduledStatementOfWorkRecordStoreFindsWhatItSaved();
    }
}
