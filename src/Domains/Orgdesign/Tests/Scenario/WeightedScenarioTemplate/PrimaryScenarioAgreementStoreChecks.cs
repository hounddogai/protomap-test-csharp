namespace Acme.Workforce.Orgdesign.Tests.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

/// <summary>Checks of weighted scenario template code, run by the project's runner.</summary>
public static class PrimaryScenarioAgreementStoreChecks
{
    public static void MemoryPrimaryScenarioAgreementStoreFindsWhatItSaved()
    {
        var store = PrimaryScenarioAgreementStore.NewMemoryPrimaryScenarioAgreementStore();
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("code-927", "label-906");
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
        MemoryPrimaryScenarioAgreementStoreFindsWhatItSaved();
    }
}
