namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentNoticeStoreChecks
{
    public static void MemoryAdjustmentNoticeStoreFindsWhatItSaved()
    {
        var store = AdjustmentNoticeStore.NewMemoryAdjustmentNoticeStore();
        var item = AdjustmentNotice.NewAdjustmentNotice("code-393", "label-19");
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
        MemoryAdjustmentNoticeStoreFindsWhatItSaved();
    }
}
