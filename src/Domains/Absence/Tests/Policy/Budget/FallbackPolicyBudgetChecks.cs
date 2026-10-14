namespace Acme.Workforce.Absence.Tests.Policy.Budget;

using Acme.Workforce.Absence.Policy.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackPolicyBudgetChecks
{
    public static void FallbackPolicyBudgetStartsInCreated()
    {
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("code-114", "name-381");
        Check.Equal(item.Status, FallbackPolicyBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPolicyBudgetMovesToValidated()
    {
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("code-114", "name-381");
        try
        {
            item.MoveTo(FallbackPolicyBudgetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPolicyBudgetStatus.Validated, "status");
    }

    public static void FallbackPolicyBudgetRejectsMovesFromRetired()
    {
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("code-114", "name-381");
        item.Status = FallbackPolicyBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackPolicyBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackPolicyBudgetStartsInCreated();
        FallbackPolicyBudgetMovesToValidated();
        FallbackPolicyBudgetRejectsMovesFromRetired();
    }
}
