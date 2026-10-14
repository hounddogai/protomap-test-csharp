namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleSlotStoreChecks
{
    public static void MemoryMeritCycleSlotStoreFindsWhatItSaved()
    {
        var store = MeritCycleSlotStore.NewMemoryMeritCycleSlotStore();
        var item = MeritCycleSlot.NewMeritCycleSlot("code-162", "title-761");
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
        MemoryMeritCycleSlotStoreFindsWhatItSaved();
    }
}
