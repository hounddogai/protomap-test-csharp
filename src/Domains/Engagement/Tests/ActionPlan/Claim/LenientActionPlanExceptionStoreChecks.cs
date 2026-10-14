namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientActionPlanExceptionStoreChecks
{
    public static void MemoryLenientActionPlanExceptionStoreFindsWhatItSaved()
    {
        var store = LenientActionPlanExceptionStore.NewMemoryLenientActionPlanExceptionStore();
        var item = LenientActionPlanException.NewLenientActionPlanException("code-952", "label-870");
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
        MemoryLenientActionPlanExceptionStoreFindsWhatItSaved();
    }
}
