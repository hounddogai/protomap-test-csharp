namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallReservationStoreChecks
{
    public static void MemoryTownHallReservationStoreFindsWhatItSaved()
    {
        var store = TownHallReservationStore.NewMemoryTownHallReservationStore();
        var item = TownHallReservation.NewTownHallReservation("code-796", "label-370");
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
        MemoryTownHallReservationStoreFindsWhatItSaved();
    }
}
