namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryDeductionBudgetStoreChecks
{
    public static void MemoryPrimaryDeductionBudgetStoreFindsWhatItSaved()
    {
        var store = PrimaryDeductionBudgetStore.NewMemoryPrimaryDeductionBudgetStore();
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("code-869", "name-738");
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
        MemoryPrimaryDeductionBudgetStoreFindsWhatItSaved();
    }
}
