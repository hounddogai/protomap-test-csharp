namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredActionPlanReservationStoreChecks
{
    public static void MemoryTieredActionPlanReservationStoreFindsWhatItSaved()
    {
        var store = TieredActionPlanReservationStore.NewMemoryTieredActionPlanReservationStore();
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("code-161", "name-536");
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
        MemoryTieredActionPlanReservationStoreFindsWhatItSaved();
    }
}
