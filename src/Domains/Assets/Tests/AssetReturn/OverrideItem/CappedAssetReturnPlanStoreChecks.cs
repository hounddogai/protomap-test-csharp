namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedAssetReturnPlanStoreChecks
{
    public static void MemoryCappedAssetReturnPlanStoreFindsWhatItSaved()
    {
        var store = CappedAssetReturnPlanStore.NewMemoryCappedAssetReturnPlanStore();
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("code-933", "name-463");
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
        MemoryCappedAssetReturnPlanStoreFindsWhatItSaved();
    }
}
