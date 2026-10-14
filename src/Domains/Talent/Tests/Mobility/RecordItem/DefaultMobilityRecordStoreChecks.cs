namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultMobilityRecordStoreChecks
{
    public static void MemoryDefaultMobilityRecordStoreFindsWhatItSaved()
    {
        var store = DefaultMobilityRecordStore.NewMemoryDefaultMobilityRecordStore();
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("code-121", "label-14");
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
        MemoryDefaultMobilityRecordStoreFindsWhatItSaved();
    }
}
