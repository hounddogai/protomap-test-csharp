namespace Acme.Workforce.Orgdesign.Tests.Scenario.Request;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalScenarioRequestStoreChecks
{
    public static void MemoryGlobalScenarioRequestStoreFindsWhatItSaved()
    {
        var store = GlobalScenarioRequestStore.NewMemoryGlobalScenarioRequestStore();
        var item = GlobalScenarioRequest.NewGlobalScenarioRequest("code-39", "title-655");
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
        MemoryGlobalScenarioRequestStoreFindsWhatItSaved();
    }
}
