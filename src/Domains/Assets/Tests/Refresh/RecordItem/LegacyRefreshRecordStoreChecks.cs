namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRefreshRecordStoreChecks
{
    public static void MemoryLegacyRefreshRecordStoreFindsWhatItSaved()
    {
        var store = LegacyRefreshRecordStore.NewMemoryLegacyRefreshRecordStore();
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("code-726", "label-838");
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
        MemoryLegacyRefreshRecordStoreFindsWhatItSaved();
    }
}
