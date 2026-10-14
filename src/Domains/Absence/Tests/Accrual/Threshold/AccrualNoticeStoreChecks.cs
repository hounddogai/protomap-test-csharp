namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualNoticeStoreChecks
{
    public static void MemoryAccrualNoticeStoreFindsWhatItSaved()
    {
        var store = AccrualNoticeStore.NewMemoryAccrualNoticeStore();
        var item = AccrualNotice.NewAccrualNotice("code-454", "label-524");
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
        MemoryAccrualNoticeStoreFindsWhatItSaved();
    }
}
