namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleRecordStoreChecks
{
    public static void MemoryMeritCycleRecordStoreFindsWhatItSaved()
    {
        var store = MeritCycleRecordStore.NewMemoryMeritCycleRecordStore();
        var item = MeritCycleRecord.NewMeritCycleRecord("code-193", "name-322");
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
        MemoryMeritCycleRecordStoreFindsWhatItSaved();
    }
}
