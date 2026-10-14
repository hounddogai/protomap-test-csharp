namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandAllocationStoreChecks
{
    public static void MemoryPayBandAllocationStoreFindsWhatItSaved()
    {
        var store = PayBandAllocationStore.NewMemoryPayBandAllocationStore();
        var item = PayBandAllocation.NewPayBandAllocation("code-811", "title-576");
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
        MemoryPayBandAllocationStoreFindsWhatItSaved();
    }
}
