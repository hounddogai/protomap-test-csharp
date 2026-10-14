namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class WeightedMobilityRecordStoreChecks
{
    public static void MemoryWeightedMobilityRecordStoreFindsWhatItSaved()
    {
        var store = WeightedMobilityRecordStore.NewMemoryWeightedMobilityRecordStore();
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("code-479", "name-285");
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
        MemoryWeightedMobilityRecordStoreFindsWhatItSaved();
    }
}
