namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RefreshWindowStoreChecks
{
    public static void MemoryRefreshWindowStoreFindsWhatItSaved()
    {
        var store = RefreshWindowStore.NewMemoryRefreshWindowStore();
        var item = RefreshWindow.NewRefreshWindow("code-698", "title-985");
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
        MemoryRefreshWindowStoreFindsWhatItSaved();
    }
}
