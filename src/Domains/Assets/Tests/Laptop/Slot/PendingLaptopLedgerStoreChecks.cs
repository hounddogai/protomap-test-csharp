namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PendingLaptopLedgerStoreChecks
{
    public static void MemoryPendingLaptopLedgerStoreFindsWhatItSaved()
    {
        var store = PendingLaptopLedgerStore.NewMemoryPendingLaptopLedgerStore();
        var item = PendingLaptopLedger.NewPendingLaptopLedger("code-853", "name-562");
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
        MemoryPendingLaptopLedgerStoreFindsWhatItSaved();
    }
}
