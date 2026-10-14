namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskBudgetStoreChecks
{
    public static void MemoryDeskBudgetStoreFindsWhatItSaved()
    {
        var store = DeskBudgetStore.NewMemoryDeskBudgetStore();
        var item = DeskBudget.NewDeskBudget("code-669", "label-55");
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
        MemoryDeskBudgetStoreFindsWhatItSaved();
    }
}
