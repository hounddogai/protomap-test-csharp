namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class GlobalPayBandBaselineStoreChecks
{
    public static void MemoryGlobalPayBandBaselineStoreFindsWhatItSaved()
    {
        var store = GlobalPayBandBaselineStore.NewMemoryGlobalPayBandBaselineStore();
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("code-754", "name-319");
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
        MemoryGlobalPayBandBaselineStoreFindsWhatItSaved();
    }
}
