namespace Acme.Workforce.Compensation.Tests.PayBand.BandTier;

using Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Checks of band tier code, run by the project's runner.</summary>
public static class PendingPayBandTierStoreChecks
{
    public static void MemoryPendingPayBandTierStoreFindsWhatItSaved()
    {
        var store = PendingPayBandTierStore.NewMemoryPendingPayBandTierStore();
        var item = PendingPayBandTier.NewPendingPayBandTier("code-131", "label-985");
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
        MemoryPendingPayBandTierStoreFindsWhatItSaved();
    }
}
