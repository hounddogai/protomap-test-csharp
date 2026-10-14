namespace Acme.Workforce.Payrollops.Tests.Deduction.Review;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualDeductionNoticeStoreChecks
{
    public static void MemoryManualDeductionNoticeStoreFindsWhatItSaved()
    {
        var store = ManualDeductionNoticeStore.NewMemoryManualDeductionNoticeStore();
        var item = ManualDeductionNotice.NewManualDeductionNotice("code-503", "title-976");
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
        MemoryManualDeductionNoticeStoreFindsWhatItSaved();
    }
}
