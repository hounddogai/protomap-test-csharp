namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class ScheduledAccrualBaselineStoreChecks
{
    public static void MemoryScheduledAccrualBaselineStoreFindsWhatItSaved()
    {
        var store = ScheduledAccrualBaselineStore.NewMemoryScheduledAccrualBaselineStore();
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("code-755", "title-589");
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
        MemoryScheduledAccrualBaselineStoreFindsWhatItSaved();
    }
}
