namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnSummaryStoreChecks
{
    public static void MemoryAssetReturnSummaryStoreFindsWhatItSaved()
    {
        var store = AssetReturnSummaryStore.NewMemoryAssetReturnSummaryStore();
        var item = AssetReturnSummary.NewAssetReturnSummary("code-98", "label-790");
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
        MemoryAssetReturnSummaryStoreFindsWhatItSaved();
    }
}
