namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class CareerPathEstimateStoreChecks
{
    public static void MemoryCareerPathEstimateStoreFindsWhatItSaved()
    {
        var store = CareerPathEstimateStore.NewMemoryCareerPathEstimateStore();
        var item = CareerPathEstimate.NewCareerPathEstimate("code-680", "name-611");
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
        MemoryCareerPathEstimateStoreFindsWhatItSaved();
    }
}
