namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class OffboardingPlanStoreChecks
{
    public static void MemoryOffboardingPlanStoreFindsWhatItSaved()
    {
        var store = OffboardingPlanStore.NewMemoryOffboardingPlanStore();
        var item = OffboardingPlan.NewOffboardingPlan("code-69", "label-649");
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
        MemoryOffboardingPlanStoreFindsWhatItSaved();
    }
}
