namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PendingScenarioAgreementStoreChecks
{
    public static void MemoryPendingScenarioAgreementStoreFindsWhatItSaved()
    {
        var store = PendingScenarioAgreementStore.NewMemoryPendingScenarioAgreementStore();
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("code-54", "label-9");
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
        MemoryPendingScenarioAgreementStoreFindsWhatItSaved();
    }
}
