namespace Acme.Workforce.Orgdesign.Tests.CostModel.Variance;

using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveCostModelPolicyStoreChecks
{
    public static void MemoryAdaptiveCostModelPolicyStoreFindsWhatItSaved()
    {
        var store = AdaptiveCostModelPolicyStore.NewMemoryAdaptiveCostModelPolicyStore();
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("code-769", "label-350");
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
        MemoryAdaptiveCostModelPolicyStoreFindsWhatItSaved();
    }
}
