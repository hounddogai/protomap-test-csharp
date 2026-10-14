namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictHeadcountReservationStoreChecks
{
    public static void MemoryStrictHeadcountReservationStoreFindsWhatItSaved()
    {
        var store = StrictHeadcountReservationStore.NewMemoryStrictHeadcountReservationStore();
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("code-708", "label-448");
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
        MemoryStrictHeadcountReservationStoreFindsWhatItSaved();
    }
}
