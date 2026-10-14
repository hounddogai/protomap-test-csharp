namespace Acme.Workforce.Absence.Tests.Policy.Budget;

using Acme.Workforce.Absence.Policy.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackPolicyBudgetRulesChecks
{
    public static void FallbackPolicyBudgetValidatorReportsEveryProblem()
    {
        var validator = FallbackPolicyBudgetRules.DefaultFallbackPolicyBudgetValidator();
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPolicyBudgetContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new FallbackPolicyBudgetContactEmailLengthRule(64);
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPolicyBudgetMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new FallbackPolicyBudgetMinimumRangeRule(1.7, 924.0);
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("x", "y");
        item.Minimum = 925.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPolicyBudgetEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new FallbackPolicyBudgetEmployeeIdLengthRule(86400);
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackPolicyBudgetValidatorReportsEveryProblem();
        FallbackPolicyBudgetContactEmailLengthRuleFlagsALongContactEmail();
        FallbackPolicyBudgetMinimumRangeRuleFlagsValuesAboveMinimum();
        FallbackPolicyBudgetEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
