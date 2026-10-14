namespace Acme.Workforce.Engagement.Tests.TownHall.Bundle;

using Acme.Workforce.Engagement.TownHall.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class DefaultTownHallPlanStoreChecks
{
    public static void MemoryDefaultTownHallPlanStoreFindsWhatItSaved()
    {
        var store = DefaultTownHallPlanStore.NewMemoryDefaultTownHallPlanStore();
        var item = DefaultTownHallPlan.NewDefaultTownHallPlan("code-686", "name-724");
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
        MemoryDefaultTownHallPlanStoreFindsWhatItSaved();
    }
}
