namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class AdaptiveOccupancyTemplateStoreChecks
{
    public static void MemoryAdaptiveOccupancyTemplateStoreFindsWhatItSaved()
    {
        var store = AdaptiveOccupancyTemplateStore.NewMemoryAdaptiveOccupancyTemplateStore();
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("code-680", "label-203");
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
        MemoryAdaptiveOccupancyTemplateStoreFindsWhatItSaved();
    }
}
