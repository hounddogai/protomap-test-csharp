namespace Acme.Workforce.Assets.Tests.Refresh.Checkpoint;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RefreshTemplateStoreChecks
{
    public static void MemoryRefreshTemplateStoreFindsWhatItSaved()
    {
        var store = RefreshTemplateStore.NewMemoryRefreshTemplateStore();
        var item = RefreshTemplate.NewRefreshTemplate("code-406", "title-636");
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
        MemoryRefreshTemplateStoreFindsWhatItSaved();
    }
}
