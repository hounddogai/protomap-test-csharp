namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class PrimaryDeductionOverrideStoreChecks
{
    public static void MemoryPrimaryDeductionOverrideStoreFindsWhatItSaved()
    {
        var store = PrimaryDeductionOverrideStore.NewMemoryPrimaryDeductionOverrideStore();
        var item = PrimaryDeductionOverride.NewPrimaryDeductionOverride("code-408", "name-175");
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
        MemoryPrimaryDeductionOverrideStoreFindsWhatItSaved();
    }
}
