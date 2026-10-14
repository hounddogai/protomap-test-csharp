namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RetroPayVarianceStoreChecks
{
    public static void MemoryRetroPayVarianceStoreFindsWhatItSaved()
    {
        var store = RetroPayVarianceStore.NewMemoryRetroPayVarianceStore();
        var item = RetroPayVariance.NewRetroPayVariance("code-783", "name-309");
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
        MemoryRetroPayVarianceStoreFindsWhatItSaved();
    }
}
