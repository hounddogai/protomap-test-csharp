namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccessBadgeReservationStoreChecks
{
    public static void MemoryAccessBadgeReservationStoreFindsWhatItSaved()
    {
        var store = AccessBadgeReservationStore.NewMemoryAccessBadgeReservationStore();
        var item = AccessBadgeReservation.NewAccessBadgeReservation("code-917", "label-786");
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
        MemoryAccessBadgeReservationStoreFindsWhatItSaved();
    }
}
