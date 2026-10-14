namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientDeductionNoticeStoreChecks
{
    public static void MemoryLenientDeductionNoticeStoreFindsWhatItSaved()
    {
        var store = LenientDeductionNoticeStore.NewMemoryLenientDeductionNoticeStore();
        var item = LenientDeductionNotice.NewLenientDeductionNotice("code-779", "label-697");
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
        MemoryLenientDeductionNoticeStoreFindsWhatItSaved();
    }
}
