namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPayBandEstimateStoreChecks
{
    public static void MemoryTieredPayBandEstimateStoreFindsWhatItSaved()
    {
        var store = TieredPayBandEstimateStore.NewMemoryTieredPayBandEstimateStore();
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("code-627", "label-771");
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
        MemoryTieredPayBandEstimateStoreFindsWhatItSaved();
    }
}
