namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientSuccessionPlanTargetStoreChecks
{
    public static void MemoryLenientSuccessionPlanTargetStoreFindsWhatItSaved()
    {
        var store = LenientSuccessionPlanTargetStore.NewMemoryLenientSuccessionPlanTargetStore();
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("code-131", "title-438");
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
        MemoryLenientSuccessionPlanTargetStoreFindsWhatItSaved();
    }
}
