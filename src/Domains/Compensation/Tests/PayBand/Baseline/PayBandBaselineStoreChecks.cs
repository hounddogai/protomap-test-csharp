namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandBaselineStoreChecks
{
    public static void MemoryPayBandBaselineStoreFindsWhatItSaved()
    {
        var store = PayBandBaselineStore.NewMemoryPayBandBaselineStore();
        var item = PayBandBaseline.NewPayBandBaseline("code-342", "title-257");
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
        MemoryPayBandBaselineStoreFindsWhatItSaved();
    }
}
