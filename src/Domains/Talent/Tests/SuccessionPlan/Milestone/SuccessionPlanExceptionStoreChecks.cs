namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanExceptionStoreChecks
{
    public static void MemorySuccessionPlanExceptionStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanExceptionStore.NewMemorySuccessionPlanExceptionStore();
        var item = SuccessionPlanException.NewSuccessionPlanException("code-152", "title-196");
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
        MemorySuccessionPlanExceptionStoreFindsWhatItSaved();
    }
}
