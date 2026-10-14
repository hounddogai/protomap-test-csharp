namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionRecordStoreChecks
{
    public static void MemoryPositionRecordStoreFindsWhatItSaved()
    {
        var store = PositionRecordStore.NewMemoryPositionRecordStore();
        var item = PositionRecord.NewPositionRecord("code-490", "name-855");
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
        MemoryPositionRecordStoreFindsWhatItSaved();
    }
}
