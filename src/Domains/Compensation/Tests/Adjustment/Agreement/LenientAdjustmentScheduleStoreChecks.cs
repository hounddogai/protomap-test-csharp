namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientAdjustmentScheduleStoreChecks
{
    public static void MemoryLenientAdjustmentScheduleStoreFindsWhatItSaved()
    {
        var store = LenientAdjustmentScheduleStore.NewMemoryLenientAdjustmentScheduleStore();
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("code-832", "name-223");
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
        MemoryLenientAdjustmentScheduleStoreFindsWhatItSaved();
    }
}
