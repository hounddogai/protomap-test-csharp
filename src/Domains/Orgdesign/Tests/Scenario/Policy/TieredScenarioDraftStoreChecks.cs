namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TieredScenarioDraftStoreChecks
{
    public static void MemoryTieredScenarioDraftStoreFindsWhatItSaved()
    {
        var store = TieredScenarioDraftStore.NewMemoryTieredScenarioDraftStore();
        var item = TieredScenarioDraft.NewTieredScenarioDraft("code-290", "label-834");
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
        MemoryTieredScenarioDraftStoreFindsWhatItSaved();
    }
}
