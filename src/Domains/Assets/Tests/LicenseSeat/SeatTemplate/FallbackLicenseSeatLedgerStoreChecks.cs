namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class FallbackLicenseSeatLedgerStoreChecks
{
    public static void MemoryFallbackLicenseSeatLedgerStoreFindsWhatItSaved()
    {
        var store = FallbackLicenseSeatLedgerStore.NewMemoryFallbackLicenseSeatLedgerStore();
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("code-28", "title-103");
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
        MemoryFallbackLicenseSeatLedgerStoreFindsWhatItSaved();
    }
}
