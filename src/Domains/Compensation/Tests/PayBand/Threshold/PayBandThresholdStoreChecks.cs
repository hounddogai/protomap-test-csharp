namespace Acme.Workforce.Compensation.Tests.PayBand.Threshold;

using Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayBandThresholdStoreChecks
{
    public static void MemoryPayBandThresholdStoreFindsWhatItSaved()
    {
        var store = PayBandThresholdStore.NewMemoryPayBandThresholdStore();
        var item = PayBandThreshold.NewPayBandThreshold("code-921", "label-526");
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
        MemoryPayBandThresholdStoreFindsWhatItSaved();
    }
}
