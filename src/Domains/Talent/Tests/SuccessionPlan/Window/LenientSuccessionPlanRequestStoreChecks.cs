namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientSuccessionPlanRequestStoreChecks
{
    public static void MemoryLenientSuccessionPlanRequestStoreFindsWhatItSaved()
    {
        var store = LenientSuccessionPlanRequestStore.NewMemoryLenientSuccessionPlanRequestStore();
        var item = LenientSuccessionPlanRequest.NewLenientSuccessionPlanRequest("code-692", "title-679");
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
        MemoryLenientSuccessionPlanRequestStoreFindsWhatItSaved();
    }
}
