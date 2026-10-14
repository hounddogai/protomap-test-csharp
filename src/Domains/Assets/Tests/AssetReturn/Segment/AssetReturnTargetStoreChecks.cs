namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AssetReturnTargetStoreChecks
{
    public static void MemoryAssetReturnTargetStoreFindsWhatItSaved()
    {
        var store = AssetReturnTargetStore.NewMemoryAssetReturnTargetStore();
        var item = AssetReturnTarget.NewAssetReturnTarget("code-619", "label-621");
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
        MemoryAssetReturnTargetStoreFindsWhatItSaved();
    }
}
