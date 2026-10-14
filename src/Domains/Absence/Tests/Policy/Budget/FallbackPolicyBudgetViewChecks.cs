namespace Acme.Workforce.Absence.Tests.Policy.Budget;

using Acme.Workforce.Absence.Policy.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackPolicyBudgetViewChecks
{
    public static void FallbackPolicyBudgetViewShowsTheStatusInCapitals()
    {
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("code-114", "name-381");
        var view = FallbackPolicyBudgetView.ToFallbackPolicyBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPolicyBudgetView.ToFallbackPolicyBudgetViews(new List<FallbackPolicyBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPolicyBudgetViewShowsTheStatusInCapitals();
    }
}
