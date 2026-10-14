namespace Acme.Workforce.Orgdesign.Tests.Scenario.Template;

using Acme.Workforce.Orgdesign.Scenario.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScenarioLedgerStoreChecks
{
    public static void MemoryScenarioLedgerStoreFindsWhatItSaved()
    {
        var store = ScenarioLedgerStore.NewMemoryScenarioLedgerStore();
        var item = ScenarioLedger.NewScenarioLedger("code-799", "title-328");
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
        MemoryScenarioLedgerStoreFindsWhatItSaved();
    }
}
