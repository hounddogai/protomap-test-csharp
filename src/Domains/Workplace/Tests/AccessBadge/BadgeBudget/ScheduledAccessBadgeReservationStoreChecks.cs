namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class ScheduledAccessBadgeReservationStoreChecks
{
    public static void MemoryScheduledAccessBadgeReservationStoreFindsWhatItSaved()
    {
        var store = ScheduledAccessBadgeReservationStore.NewMemoryScheduledAccessBadgeReservationStore();
        var item = ScheduledAccessBadgeReservation.NewScheduledAccessBadgeReservation("code-235", "title-924");
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
        MemoryScheduledAccessBadgeReservationStoreFindsWhatItSaved();
    }
}
