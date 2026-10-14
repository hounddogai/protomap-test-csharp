namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredLaptopNoticeStoreChecks
{
    public static void MemoryTieredLaptopNoticeStoreFindsWhatItSaved()
    {
        var store = TieredLaptopNoticeStore.NewMemoryTieredLaptopNoticeStore();
        var item = TieredLaptopNotice.NewTieredLaptopNotice("code-430", "title-900");
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
        MemoryTieredLaptopNoticeStoreFindsWhatItSaved();
    }
}
