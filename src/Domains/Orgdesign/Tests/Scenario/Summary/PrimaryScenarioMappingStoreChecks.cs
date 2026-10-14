namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PrimaryScenarioMappingStoreChecks
{
    public static void MemoryPrimaryScenarioMappingStoreFindsWhatItSaved()
    {
        var store = PrimaryScenarioMappingStore.NewMemoryPrimaryScenarioMappingStore();
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("code-657", "label-827");
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
        MemoryPrimaryScenarioMappingStoreFindsWhatItSaved();
    }
}
