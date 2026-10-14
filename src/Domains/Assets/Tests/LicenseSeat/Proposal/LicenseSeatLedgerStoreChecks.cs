namespace Acme.Workforce.Assets.Tests.LicenseSeat.Proposal;

using Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LicenseSeatLedgerStoreChecks
{
    public static void MemoryLicenseSeatLedgerStoreFindsWhatItSaved()
    {
        var store = LicenseSeatLedgerStore.NewMemoryLicenseSeatLedgerStore();
        var item = LicenseSeatLedger.NewLicenseSeatLedger("code-34", "label-399");
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
        MemoryLicenseSeatLedgerStoreFindsWhatItSaved();
    }
}
