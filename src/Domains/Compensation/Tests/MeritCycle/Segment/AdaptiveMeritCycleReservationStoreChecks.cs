namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleReservationStoreChecks
{
    public static void MemoryAdaptiveMeritCycleReservationStoreFindsWhatItSaved()
    {
        var store = AdaptiveMeritCycleReservationStore.NewMemoryAdaptiveMeritCycleReservationStore();
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("code-287", "title-79");
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
        MemoryAdaptiveMeritCycleReservationStoreFindsWhatItSaved();
    }
}
