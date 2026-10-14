namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GlobalDeductionReservationStoreChecks
{
    public static void MemoryGlobalDeductionReservationStoreFindsWhatItSaved()
    {
        var store = GlobalDeductionReservationStore.NewMemoryGlobalDeductionReservationStore();
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("code-479", "title-418");
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
        MemoryGlobalDeductionReservationStoreFindsWhatItSaved();
    }
}
