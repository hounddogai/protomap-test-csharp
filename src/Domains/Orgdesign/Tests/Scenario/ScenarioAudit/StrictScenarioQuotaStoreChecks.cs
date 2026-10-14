namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class StrictScenarioQuotaStoreChecks
{
    public static void MemoryStrictScenarioQuotaStoreFindsWhatItSaved()
    {
        var store = StrictScenarioQuotaStore.NewMemoryStrictScenarioQuotaStore();
        var item = StrictScenarioQuota.NewStrictScenarioQuota("code-509", "name-50");
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
        MemoryStrictScenarioQuotaStoreFindsWhatItSaved();
    }
}
