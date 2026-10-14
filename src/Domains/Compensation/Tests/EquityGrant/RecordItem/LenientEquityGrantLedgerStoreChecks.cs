namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientEquityGrantLedgerStoreChecks
{
    public static void MemoryLenientEquityGrantLedgerStoreFindsWhatItSaved()
    {
        var store = LenientEquityGrantLedgerStore.NewMemoryLenientEquityGrantLedgerStore();
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("code-416", "title-25");
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
        MemoryLenientEquityGrantLedgerStoreFindsWhatItSaved();
    }
}
