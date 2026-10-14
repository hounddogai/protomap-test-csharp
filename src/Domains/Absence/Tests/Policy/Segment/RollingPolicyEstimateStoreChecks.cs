namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingPolicyEstimateStoreChecks
{
    public static void MemoryRollingPolicyEstimateStoreFindsWhatItSaved()
    {
        var store = RollingPolicyEstimateStore.NewMemoryRollingPolicyEstimateStore();
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("code-459", "title-587");
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
        MemoryRollingPolicyEstimateStoreFindsWhatItSaved();
    }
}
