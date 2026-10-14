namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class ScheduledCostModelPolicyStoreChecks
{
    public static void MemoryScheduledCostModelPolicyStoreFindsWhatItSaved()
    {
        var store = ScheduledCostModelPolicyStore.NewMemoryScheduledCostModelPolicyStore();
        var item = ScheduledCostModelPolicy.NewScheduledCostModelPolicy("code-932", "title-170");
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
        MemoryScheduledCostModelPolicyStoreFindsWhatItSaved();
    }
}
