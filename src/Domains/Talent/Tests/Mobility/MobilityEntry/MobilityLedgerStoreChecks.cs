namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class MobilityLedgerStoreChecks
{
    public static void MemoryMobilityLedgerStoreFindsWhatItSaved()
    {
        var store = MobilityLedgerStore.NewMemoryMobilityLedgerStore();
        var item = MobilityLedger.NewMobilityLedger("code-226", "title-380");
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
        MemoryMobilityLedgerStoreFindsWhatItSaved();
    }
}
