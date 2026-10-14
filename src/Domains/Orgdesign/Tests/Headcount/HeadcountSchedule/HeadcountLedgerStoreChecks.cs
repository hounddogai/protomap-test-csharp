namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class HeadcountLedgerStoreChecks
{
    public static void MemoryHeadcountLedgerStoreFindsWhatItSaved()
    {
        var store = HeadcountLedgerStore.NewMemoryHeadcountLedgerStore();
        var item = HeadcountLedger.NewHeadcountLedger("code-656", "title-1");
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
        MemoryHeadcountLedgerStoreFindsWhatItSaved();
    }
}
