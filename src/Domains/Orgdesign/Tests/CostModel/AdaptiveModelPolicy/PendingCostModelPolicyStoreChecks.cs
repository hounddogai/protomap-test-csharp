namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class PendingCostModelPolicyStoreChecks
{
    public static void MemoryPendingCostModelPolicyStoreFindsWhatItSaved()
    {
        var store = PendingCostModelPolicyStore.NewMemoryPendingCostModelPolicyStore();
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("code-411", "label-378");
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
        MemoryPendingCostModelPolicyStoreFindsWhatItSaved();
    }
}
