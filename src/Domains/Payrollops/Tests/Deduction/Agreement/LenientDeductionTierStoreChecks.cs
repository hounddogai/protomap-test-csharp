namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionTierStoreChecks
{
    public static void MemoryLenientDeductionTierStoreFindsWhatItSaved()
    {
        var store = LenientDeductionTierStore.NewMemoryLenientDeductionTierStore();
        var item = LenientDeductionTier.NewLenientDeductionTier("code-380", "name-549");
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
        MemoryLenientDeductionTierStoreFindsWhatItSaved();
    }
}
