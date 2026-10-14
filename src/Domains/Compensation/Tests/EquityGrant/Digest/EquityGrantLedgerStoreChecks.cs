namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantLedgerStoreChecks
{
    public static void MemoryEquityGrantLedgerStoreFindsWhatItSaved()
    {
        var store = EquityGrantLedgerStore.NewMemoryEquityGrantLedgerStore();
        var item = EquityGrantLedger.NewEquityGrantLedger("code-125", "title-728");
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
        MemoryEquityGrantLedgerStoreFindsWhatItSaved();
    }
}
