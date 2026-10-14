namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScenarioBudgetStoreChecks
{
    public static void MemoryScenarioBudgetStoreFindsWhatItSaved()
    {
        var store = ScenarioBudgetStore.NewMemoryScenarioBudgetStore();
        var item = ScenarioBudget.NewScenarioBudget("code-279", "label-139");
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
        MemoryScenarioBudgetStoreFindsWhatItSaved();
    }
}
