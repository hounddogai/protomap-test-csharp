namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionTierStoreChecks
{
    public static void MemoryDeductionTierStoreFindsWhatItSaved()
    {
        var store = DeductionTierStore.NewMemoryDeductionTierStore();
        var item = DeductionTier.NewDeductionTier("code-163", "title-291");
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
        MemoryDeductionTierStoreFindsWhatItSaved();
    }
}
