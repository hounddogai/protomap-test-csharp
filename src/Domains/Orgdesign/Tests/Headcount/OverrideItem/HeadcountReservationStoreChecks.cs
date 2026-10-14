namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountReservationStoreChecks
{
    public static void MemoryHeadcountReservationStoreFindsWhatItSaved()
    {
        var store = HeadcountReservationStore.NewMemoryHeadcountReservationStore();
        var item = HeadcountReservation.NewHeadcountReservation("code-390", "title-713");
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
        MemoryHeadcountReservationStoreFindsWhatItSaved();
    }
}
