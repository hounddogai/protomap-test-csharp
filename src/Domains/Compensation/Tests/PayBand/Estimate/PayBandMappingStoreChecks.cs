namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayBandMappingStoreChecks
{
    public static void MemoryPayBandMappingStoreFindsWhatItSaved()
    {
        var store = PayBandMappingStore.NewMemoryPayBandMappingStore();
        var item = PayBandMapping.NewPayBandMapping("code-381", "label-10");
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
        MemoryPayBandMappingStoreFindsWhatItSaved();
    }
}
