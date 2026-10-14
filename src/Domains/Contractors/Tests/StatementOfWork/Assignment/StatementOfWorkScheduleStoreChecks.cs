namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkScheduleStoreChecks
{
    public static void MemoryStatementOfWorkScheduleStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkScheduleStore.NewMemoryStatementOfWorkScheduleStore();
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("code-569", "name-787");
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
        MemoryStatementOfWorkScheduleStoreFindsWhatItSaved();
    }
}
