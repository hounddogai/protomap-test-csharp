namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class SeasonalHeadcountReservationStoreChecks
{
    public static void MemorySeasonalHeadcountReservationStoreFindsWhatItSaved()
    {
        var store = SeasonalHeadcountReservationStore.NewMemorySeasonalHeadcountReservationStore();
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("code-430", "title-691");
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
        MemorySeasonalHeadcountReservationStoreFindsWhatItSaved();
    }
}
