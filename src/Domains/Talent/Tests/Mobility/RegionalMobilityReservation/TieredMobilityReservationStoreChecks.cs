namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class TieredMobilityReservationStoreChecks
{
    public static void MemoryTieredMobilityReservationStoreFindsWhatItSaved()
    {
        var store = TieredMobilityReservationStore.NewMemoryTieredMobilityReservationStore();
        var item = TieredMobilityReservation.NewTieredMobilityReservation("code-76", "name-656");
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
        MemoryTieredMobilityReservationStoreFindsWhatItSaved();
    }
}
