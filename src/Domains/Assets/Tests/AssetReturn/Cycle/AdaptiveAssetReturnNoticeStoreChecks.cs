namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnNoticeStoreChecks
{
    public static void MemoryAdaptiveAssetReturnNoticeStoreFindsWhatItSaved()
    {
        var store = AdaptiveAssetReturnNoticeStore.NewMemoryAdaptiveAssetReturnNoticeStore();
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("code-647", "title-900");
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
        MemoryAdaptiveAssetReturnNoticeStoreFindsWhatItSaved();
    }
}
