namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathPolicyStoreChecks
{
    public static void MemoryCareerPathPolicyStoreFindsWhatItSaved()
    {
        var store = CareerPathPolicyStore.NewMemoryCareerPathPolicyStore();
        var item = CareerPathPolicy.NewCareerPathPolicy("code-309", "label-28");
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
        MemoryCareerPathPolicyStoreFindsWhatItSaved();
    }
}
