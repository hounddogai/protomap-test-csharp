namespace Acme.Workforce.Workplace.Tests.AccessBadge.Request;

using Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class WeightedAccessBadgeReservationStoreChecks
{
    public static void MemoryWeightedAccessBadgeReservationStoreFindsWhatItSaved()
    {
        var store = WeightedAccessBadgeReservationStore.NewMemoryWeightedAccessBadgeReservationStore();
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("code-768", "title-381");
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
        MemoryWeightedAccessBadgeReservationStoreFindsWhatItSaved();
    }
}
