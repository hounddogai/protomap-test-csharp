namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class DefaultDeskBudgetStoreChecks
{
    public static void MemoryDefaultDeskBudgetStoreFindsWhatItSaved()
    {
        var store = DefaultDeskBudgetStore.NewMemoryDefaultDeskBudgetStore();
        var item = DefaultDeskBudget.NewDefaultDeskBudget("code-175", "title-303");
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
        MemoryDefaultDeskBudgetStoreFindsWhatItSaved();
    }
}
