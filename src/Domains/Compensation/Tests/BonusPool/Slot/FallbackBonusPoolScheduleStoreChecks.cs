namespace Acme.Workforce.Compensation.Tests.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class FallbackBonusPoolScheduleStoreChecks
{
    public static void MemoryFallbackBonusPoolScheduleStoreFindsWhatItSaved()
    {
        var store = FallbackBonusPoolScheduleStore.NewMemoryFallbackBonusPoolScheduleStore();
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("code-540", "name-995");
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
        MemoryFallbackBonusPoolScheduleStoreFindsWhatItSaved();
    }
}
