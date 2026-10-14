namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioProposalStoreChecks
{
    public static void MemoryScenarioProposalStoreFindsWhatItSaved()
    {
        var store = ScenarioProposalStore.NewMemoryScenarioProposalStore();
        var item = ScenarioProposal.NewScenarioProposal("code-852", "title-363");
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
        MemoryScenarioProposalStoreFindsWhatItSaved();
    }
}
