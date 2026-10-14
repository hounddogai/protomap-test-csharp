namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnScheduleStoreChecks
{
    public static void MemoryAssetReturnScheduleStoreFindsWhatItSaved()
    {
        var store = AssetReturnScheduleStore.NewMemoryAssetReturnScheduleStore();
        var item = AssetReturnSchedule.NewAssetReturnSchedule("code-482", "title-836");
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
        MemoryAssetReturnScheduleStoreFindsWhatItSaved();
    }
}
