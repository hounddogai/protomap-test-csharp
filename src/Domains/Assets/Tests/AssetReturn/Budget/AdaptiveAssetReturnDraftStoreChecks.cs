namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnDraftStoreChecks
{
    public static void MemoryAdaptiveAssetReturnDraftStoreFindsWhatItSaved()
    {
        var store = AdaptiveAssetReturnDraftStore.NewMemoryAdaptiveAssetReturnDraftStore();
        var item = AdaptiveAssetReturnDraft.NewAdaptiveAssetReturnDraft("code-741", "label-307");
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
        MemoryAdaptiveAssetReturnDraftStoreFindsWhatItSaved();
    }
}
