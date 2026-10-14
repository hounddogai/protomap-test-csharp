namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedCostModelAssignmentStoreChecks
{
    public static void MemoryCappedCostModelAssignmentStoreFindsWhatItSaved()
    {
        var store = CappedCostModelAssignmentStore.NewMemoryCappedCostModelAssignmentStore();
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("code-780", "name-182");
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
        MemoryCappedCostModelAssignmentStoreFindsWhatItSaved();
    }
}
