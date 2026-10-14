namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredOccupancyRuleStoreChecks
{
    public static void MemoryTieredOccupancyRuleStoreFindsWhatItSaved()
    {
        var store = TieredOccupancyRuleStore.NewMemoryTieredOccupancyRuleStore();
        var item = TieredOccupancyRule.NewTieredOccupancyRule("code-702", "label-675");
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
        MemoryTieredOccupancyRuleStoreFindsWhatItSaved();
    }
}
