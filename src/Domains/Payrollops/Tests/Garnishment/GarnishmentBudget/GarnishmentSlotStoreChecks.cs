namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class GarnishmentSlotStoreChecks
{
    public static void MemoryGarnishmentSlotStoreFindsWhatItSaved()
    {
        var store = GarnishmentSlotStore.NewMemoryGarnishmentSlotStore();
        var item = GarnishmentSlot.NewGarnishmentSlot("code-693", "title-683");
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
        MemoryGarnishmentSlotStoreFindsWhatItSaved();
    }
}
