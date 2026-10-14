namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioAgreementStoreChecks
{
    public static void MemoryScenarioAgreementStoreFindsWhatItSaved()
    {
        var store = ScenarioAgreementStore.NewMemoryScenarioAgreementStore();
        var item = ScenarioAgreement.NewScenarioAgreement("code-747", "name-426");
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
        MemoryScenarioAgreementStoreFindsWhatItSaved();
    }
}
