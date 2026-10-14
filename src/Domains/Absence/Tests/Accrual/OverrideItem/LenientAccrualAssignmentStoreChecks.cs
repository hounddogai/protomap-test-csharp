namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class LenientAccrualAssignmentStoreChecks
{
    public static void MemoryLenientAccrualAssignmentStoreFindsWhatItSaved()
    {
        var store = LenientAccrualAssignmentStore.NewMemoryLenientAccrualAssignmentStore();
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("code-301", "label-784");
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
        MemoryLenientAccrualAssignmentStoreFindsWhatItSaved();
    }
}
