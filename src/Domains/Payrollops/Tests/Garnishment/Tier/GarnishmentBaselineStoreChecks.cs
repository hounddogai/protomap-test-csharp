namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentBaselineStoreChecks
{
    public static void MemoryGarnishmentBaselineStoreFindsWhatItSaved()
    {
        var store = GarnishmentBaselineStore.NewMemoryGarnishmentBaselineStore();
        var item = GarnishmentBaseline.NewGarnishmentBaseline("code-836", "label-332");
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
        MemoryGarnishmentBaselineStoreFindsWhatItSaved();
    }
}
