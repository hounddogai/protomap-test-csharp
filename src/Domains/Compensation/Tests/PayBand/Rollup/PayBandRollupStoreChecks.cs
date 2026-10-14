namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandRollupStoreChecks
{
    public static void MemoryPayBandRollupStoreFindsWhatItSaved()
    {
        var store = PayBandRollupStore.NewMemoryPayBandRollupStore();
        var item = PayBandRollup.NewPayBandRollup("code-635", "title-282");
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
        MemoryPayBandRollupStoreFindsWhatItSaved();
    }
}
