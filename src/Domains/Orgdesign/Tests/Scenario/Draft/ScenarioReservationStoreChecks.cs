namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScenarioReservationStoreChecks
{
    public static void MemoryScenarioReservationStoreFindsWhatItSaved()
    {
        var store = ScenarioReservationStore.NewMemoryScenarioReservationStore();
        var item = ScenarioReservation.NewScenarioReservation("code-546", "title-817");
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
        MemoryScenarioReservationStoreFindsWhatItSaved();
    }
}
