namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class PrimaryScenarioTierStoreChecks
{
    public static void MemoryPrimaryScenarioTierStoreFindsWhatItSaved()
    {
        var store = PrimaryScenarioTierStore.NewMemoryPrimaryScenarioTierStore();
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("code-842", "title-255");
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
        MemoryPrimaryScenarioTierStoreFindsWhatItSaved();
    }
}
