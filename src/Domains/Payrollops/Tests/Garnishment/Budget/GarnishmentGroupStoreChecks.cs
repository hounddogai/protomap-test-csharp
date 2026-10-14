namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class GarnishmentGroupStoreChecks
{
    public static void MemoryGarnishmentGroupStoreFindsWhatItSaved()
    {
        var store = GarnishmentGroupStore.NewMemoryGarnishmentGroupStore();
        var item = GarnishmentGroup.NewGarnishmentGroup("code-915", "name-903");
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
        MemoryGarnishmentGroupStoreFindsWhatItSaved();
    }
}
