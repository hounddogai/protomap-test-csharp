namespace Acme.Workforce.Payrollops.Tests.Deduction.Score;

using Acme.Workforce.Payrollops.Deduction.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledDeductionEstimateStoreChecks
{
    public static void MemoryScheduledDeductionEstimateStoreFindsWhatItSaved()
    {
        var store = ScheduledDeductionEstimateStore.NewMemoryScheduledDeductionEstimateStore();
        var item = ScheduledDeductionEstimate.NewScheduledDeductionEstimate("code-558", "name-565");
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
        MemoryScheduledDeductionEstimateStoreFindsWhatItSaved();
    }
}
