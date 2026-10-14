namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayPlanStoreChecks
{
    public static void MemoryRetroPayPlanStoreFindsWhatItSaved()
    {
        var store = RetroPayPlanStore.NewMemoryRetroPayPlanStore();
        var item = RetroPayPlan.NewRetroPayPlan("code-539", "name-23");
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
        MemoryRetroPayPlanStoreFindsWhatItSaved();
    }
}
