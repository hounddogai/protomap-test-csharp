namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientAssetReturnTargetStoreChecks
{
    public static void MemoryLenientAssetReturnTargetStoreFindsWhatItSaved()
    {
        var store = LenientAssetReturnTargetStore.NewMemoryLenientAssetReturnTargetStore();
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("code-566", "name-521");
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
        MemoryLenientAssetReturnTargetStoreFindsWhatItSaved();
    }
}
