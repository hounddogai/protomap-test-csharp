namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAssignmentStoreChecks
{
    public static void MemoryAccrualAssignmentStoreFindsWhatItSaved()
    {
        var store = AccrualAssignmentStore.NewMemoryAccrualAssignmentStore();
        var item = AccrualAssignment.NewAccrualAssignment("code-753", "title-491");
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
        MemoryAccrualAssignmentStoreFindsWhatItSaved();
    }
}
