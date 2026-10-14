namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationAdjustmentStoreChecks
{
    public static void MemoryNominationAdjustmentStoreFindsWhatItSaved()
    {
        var store = NominationAdjustmentStore.NewMemoryNominationAdjustmentStore();
        var item = NominationAdjustment.NewNominationAdjustment("code-988", "title-208");
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
        MemoryNominationAdjustmentStoreFindsWhatItSaved();
    }
}
