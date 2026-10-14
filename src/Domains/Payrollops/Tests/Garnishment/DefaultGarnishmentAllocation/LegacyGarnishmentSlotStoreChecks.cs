namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentSlotStoreChecks
{
    public static void MemoryLegacyGarnishmentSlotStoreFindsWhatItSaved()
    {
        var store = LegacyGarnishmentSlotStore.NewMemoryLegacyGarnishmentSlotStore();
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("code-718", "label-569");
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
        MemoryLegacyGarnishmentSlotStoreFindsWhatItSaved();
    }
}
