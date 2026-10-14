namespace Acme.Workforce.Payrollops.Tests.Garnishment.Variance;

using Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GarnishmentVarianceStoreChecks
{
    public static void MemoryGarnishmentVarianceStoreFindsWhatItSaved()
    {
        var store = GarnishmentVarianceStore.NewMemoryGarnishmentVarianceStore();
        var item = GarnishmentVariance.NewGarnishmentVariance("code-858", "label-818");
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
        MemoryGarnishmentVarianceStoreFindsWhatItSaved();
    }
}
