namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PrimaryHeadcountLedgerStoreChecks
{
    public static void MemoryPrimaryHeadcountLedgerStoreFindsWhatItSaved()
    {
        var store = PrimaryHeadcountLedgerStore.NewMemoryPrimaryHeadcountLedgerStore();
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("code-124", "title-19");
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
        MemoryPrimaryHeadcountLedgerStoreFindsWhatItSaved();
    }
}
