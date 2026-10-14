namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LegacyCostModelGroupStoreChecks
{
    public static void MemoryLegacyCostModelGroupStoreFindsWhatItSaved()
    {
        var store = LegacyCostModelGroupStore.NewMemoryLegacyCostModelGroupStore();
        var item = LegacyCostModelGroup.NewLegacyCostModelGroup("code-168", "label-705");
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
        MemoryLegacyCostModelGroupStoreFindsWhatItSaved();
    }
}
