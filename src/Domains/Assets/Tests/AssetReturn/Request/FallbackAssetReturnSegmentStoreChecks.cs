namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackAssetReturnSegmentStoreChecks
{
    public static void MemoryFallbackAssetReturnSegmentStoreFindsWhatItSaved()
    {
        var store = FallbackAssetReturnSegmentStore.NewMemoryFallbackAssetReturnSegmentStore();
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("code-403", "title-868");
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
        MemoryFallbackAssetReturnSegmentStoreFindsWhatItSaved();
    }
}
