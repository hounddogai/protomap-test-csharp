namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CareerPathPlanStoreChecks
{
    public static void MemoryCareerPathPlanStoreFindsWhatItSaved()
    {
        var store = CareerPathPlanStore.NewMemoryCareerPathPlanStore();
        var item = CareerPathPlan.NewCareerPathPlan("code-260", "label-769");
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
        MemoryCareerPathPlanStoreFindsWhatItSaved();
    }
}
