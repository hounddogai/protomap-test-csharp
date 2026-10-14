namespace Acme.Workforce.Absence.Tests.Policy.Estimate;

using Acme.Workforce.Absence.Policy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PolicyEstimateStoreChecks
{
    public static void MemoryPolicyEstimateStoreFindsWhatItSaved()
    {
        var store = PolicyEstimateStore.NewMemoryPolicyEstimateStore();
        var item = PolicyEstimate.NewPolicyEstimate("code-747", "title-911");
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
        MemoryPolicyEstimateStoreFindsWhatItSaved();
    }
}
