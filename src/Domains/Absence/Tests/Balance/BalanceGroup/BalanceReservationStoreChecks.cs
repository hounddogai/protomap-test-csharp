namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalanceReservationStoreChecks
{
    public static void MemoryBalanceReservationStoreFindsWhatItSaved()
    {
        var store = BalanceReservationStore.NewMemoryBalanceReservationStore();
        var item = BalanceReservation.NewBalanceReservation("code-418", "title-341");
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
        MemoryBalanceReservationStoreFindsWhatItSaved();
    }
}
