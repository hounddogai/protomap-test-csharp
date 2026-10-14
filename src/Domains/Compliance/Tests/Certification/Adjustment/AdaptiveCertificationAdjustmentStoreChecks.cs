namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AdaptiveCertificationAdjustmentStoreChecks
{
    public static void MemoryAdaptiveCertificationAdjustmentStoreFindsWhatItSaved()
    {
        var store = AdaptiveCertificationAdjustmentStore.NewMemoryAdaptiveCertificationAdjustmentStore();
        var item = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment("code-79", "name-158");
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
        MemoryAdaptiveCertificationAdjustmentStoreFindsWhatItSaved();
    }
}
