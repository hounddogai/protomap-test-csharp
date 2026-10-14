namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandEstimateStoreChecks
{
    public static void MemoryPayBandEstimateStoreFindsWhatItSaved()
    {
        var store = PayBandEstimateStore.NewMemoryPayBandEstimateStore();
        var item = PayBandEstimate.NewPayBandEstimate("code-610", "label-938");
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
        MemoryPayBandEstimateStoreFindsWhatItSaved();
    }
}
