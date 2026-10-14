namespace Acme.Workforce.Absence.Tests.Accrual.AccrualAssignment;

using Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>Checks of accrual assignment code, run by the project's runner.</summary>
public static class ScheduledAccrualAssignmentStoreChecks
{
    public static void MemoryScheduledAccrualAssignmentStoreFindsWhatItSaved()
    {
        var store = ScheduledAccrualAssignmentStore.NewMemoryScheduledAccrualAssignmentStore();
        var item = ScheduledAccrualAssignment.NewScheduledAccrualAssignment("code-768", "label-996");
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
        MemoryScheduledAccrualAssignmentStoreFindsWhatItSaved();
    }
}
