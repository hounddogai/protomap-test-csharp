namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Request;

using Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanRequestStoreChecks
{
    public static void MemoryGlobalSuccessionPlanRequestStoreFindsWhatItSaved()
    {
        var store = GlobalSuccessionPlanRequestStore.NewMemoryGlobalSuccessionPlanRequestStore();
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("code-808", "label-269");
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
        MemoryGlobalSuccessionPlanRequestStoreFindsWhatItSaved();
    }
}
