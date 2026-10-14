namespace Acme.Workforce.Absence.Tests.Policy.Budget;

using Acme.Workforce.Absence.Policy.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackPolicyBudgetStoreChecks
{
    public static void MemoryFallbackPolicyBudgetStoreFindsWhatItSaved()
    {
        var store = FallbackPolicyBudgetStore.NewMemoryFallbackPolicyBudgetStore();
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("code-114", "name-381");
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
        MemoryFallbackPolicyBudgetStoreFindsWhatItSaved();
    }
}
