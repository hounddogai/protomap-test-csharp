namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelAssignmentStoreChecks
{
    public static void MemoryCostModelAssignmentStoreFindsWhatItSaved()
    {
        var store = CostModelAssignmentStore.NewMemoryCostModelAssignmentStore();
        var item = CostModelAssignment.NewCostModelAssignment("code-948", "label-57");
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
        MemoryCostModelAssignmentStoreFindsWhatItSaved();
    }
}
