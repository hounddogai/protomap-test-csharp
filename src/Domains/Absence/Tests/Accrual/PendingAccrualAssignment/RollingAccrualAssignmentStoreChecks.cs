namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class RollingAccrualAssignmentStoreChecks
{
    public static void MemoryRollingAccrualAssignmentStoreFindsWhatItSaved()
    {
        var store = RollingAccrualAssignmentStore.NewMemoryRollingAccrualAssignmentStore();
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("code-491", "title-549");
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
        MemoryRollingAccrualAssignmentStoreFindsWhatItSaved();
    }
}
