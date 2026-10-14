namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GlobalAssetReturnRecordStoreChecks
{
    public static void MemoryGlobalAssetReturnRecordStoreFindsWhatItSaved()
    {
        var store = GlobalAssetReturnRecordStore.NewMemoryGlobalAssetReturnRecordStore();
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("code-611", "name-658");
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
        MemoryGlobalAssetReturnRecordStoreFindsWhatItSaved();
    }
}
