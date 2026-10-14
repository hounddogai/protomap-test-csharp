namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssetReturnRequestStoreChecks
{
    public static void MemoryAssetReturnRequestStoreFindsWhatItSaved()
    {
        var store = AssetReturnRequestStore.NewMemoryAssetReturnRequestStore();
        var item = AssetReturnRequest.NewAssetReturnRequest("code-367", "title-885");
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
        MemoryAssetReturnRequestStoreFindsWhatItSaved();
    }
}
