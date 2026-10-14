namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnPlanStoreChecks
{
    public static void MemoryAssetReturnPlanStoreFindsWhatItSaved()
    {
        var store = AssetReturnPlanStore.NewMemoryAssetReturnPlanStore();
        var item = AssetReturnPlan.NewAssetReturnPlan("code-852", "title-255");
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
        MemoryAssetReturnPlanStoreFindsWhatItSaved();
    }
}
