namespace Acme.Workforce.Orgdesign.Tests.Scenario.Audit;

using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyScenarioOverrideStoreChecks
{
    public static void MemoryLegacyScenarioOverrideStoreFindsWhatItSaved()
    {
        var store = LegacyScenarioOverrideStore.NewMemoryLegacyScenarioOverrideStore();
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("code-298", "title-53");
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
        MemoryLegacyScenarioOverrideStoreFindsWhatItSaved();
    }
}
