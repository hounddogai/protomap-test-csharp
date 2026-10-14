namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class RegionalCostModelAssignmentStoreChecks
{
    public static void MemoryRegionalCostModelAssignmentStoreFindsWhatItSaved()
    {
        var store = RegionalCostModelAssignmentStore.NewMemoryRegionalCostModelAssignmentStore();
        var item = RegionalCostModelAssignment.NewRegionalCostModelAssignment("code-579", "name-471");
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
        MemoryRegionalCostModelAssignmentStoreFindsWhatItSaved();
    }
}
