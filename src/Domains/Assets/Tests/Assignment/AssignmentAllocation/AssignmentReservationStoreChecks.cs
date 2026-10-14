namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentAllocation;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Checks of assignment allocation code, run by the project's runner.</summary>
public static class AssignmentReservationStoreChecks
{
    public static void MemoryAssignmentReservationStoreFindsWhatItSaved()
    {
        var store = AssignmentReservationStore.NewMemoryAssignmentReservationStore();
        var item = AssignmentReservation.NewAssignmentReservation("code-172", "name-795");
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
        MemoryAssignmentReservationStoreFindsWhatItSaved();
    }
}
