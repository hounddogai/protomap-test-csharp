namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanTemplateStoreChecks
{
    public static void MemoryFloorPlanTemplateStoreFindsWhatItSaved()
    {
        var store = FloorPlanTemplateStore.NewMemoryFloorPlanTemplateStore();
        var item = FloorPlanTemplate.NewFloorPlanTemplate("code-690", "label-680");
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
        MemoryFloorPlanTemplateStoreFindsWhatItSaved();
    }
}
