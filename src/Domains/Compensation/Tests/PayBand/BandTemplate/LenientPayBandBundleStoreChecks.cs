namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class LenientPayBandBundleStoreChecks
{
    public static void MemoryLenientPayBandBundleStoreFindsWhatItSaved()
    {
        var store = LenientPayBandBundleStore.NewMemoryLenientPayBandBundleStore();
        var item = LenientPayBandBundle.NewLenientPayBandBundle("code-54", "label-164");
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
        MemoryLenientPayBandBundleStoreFindsWhatItSaved();
    }
}
