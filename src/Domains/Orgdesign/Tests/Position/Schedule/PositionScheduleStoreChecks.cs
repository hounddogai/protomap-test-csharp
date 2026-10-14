namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PositionScheduleStoreChecks
{
    public static void MemoryPositionScheduleStoreFindsWhatItSaved()
    {
        var store = PositionScheduleStore.NewMemoryPositionScheduleStore();
        var item = PositionSchedule.NewPositionSchedule("code-938", "label-989");
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
        MemoryPositionScheduleStoreFindsWhatItSaved();
    }
}
