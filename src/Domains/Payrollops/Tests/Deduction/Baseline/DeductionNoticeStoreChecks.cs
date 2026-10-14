namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionNoticeStoreChecks
{
    public static void MemoryDeductionNoticeStoreFindsWhatItSaved()
    {
        var store = DeductionNoticeStore.NewMemoryDeductionNoticeStore();
        var item = DeductionNotice.NewDeductionNotice("code-369", "label-395");
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
        MemoryDeductionNoticeStoreFindsWhatItSaved();
    }
}
