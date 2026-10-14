namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictReorgBudgetStoreChecks
{
    public static void MemoryStrictReorgBudgetStoreFindsWhatItSaved()
    {
        var store = StrictReorgBudgetStore.NewMemoryStrictReorgBudgetStore();
        var item = StrictReorgBudget.NewStrictReorgBudget("code-640", "name-345");
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
        MemoryStrictReorgBudgetStoreFindsWhatItSaved();
    }
}
