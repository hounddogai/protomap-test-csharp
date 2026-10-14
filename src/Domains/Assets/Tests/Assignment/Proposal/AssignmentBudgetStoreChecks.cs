namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssignmentBudgetStoreChecks
{
    public static void MemoryAssignmentBudgetStoreFindsWhatItSaved()
    {
        var store = AssignmentBudgetStore.NewMemoryAssignmentBudgetStore();
        var item = AssignmentBudget.NewAssignmentBudget("code-382", "label-695");
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
        MemoryAssignmentBudgetStoreFindsWhatItSaved();
    }
}
