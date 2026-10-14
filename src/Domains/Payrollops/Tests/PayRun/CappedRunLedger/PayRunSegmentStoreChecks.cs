namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class PayRunSegmentStoreChecks
{
    public static void MemoryPayRunSegmentStoreFindsWhatItSaved()
    {
        var store = PayRunSegmentStore.NewMemoryPayRunSegmentStore();
        var item = PayRunSegment.NewPayRunSegment("code-355", "label-230");
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
        MemoryPayRunSegmentStoreFindsWhatItSaved();
    }
}
