namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationReservationStoreChecks
{
    public static void MemoryScheduledNominationReservationStoreFindsWhatItSaved()
    {
        var store = ScheduledNominationReservationStore.NewMemoryScheduledNominationReservationStore();
        var item = ScheduledNominationReservation.NewScheduledNominationReservation("code-669", "title-600");
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
        MemoryScheduledNominationReservationStoreFindsWhatItSaved();
    }
}
