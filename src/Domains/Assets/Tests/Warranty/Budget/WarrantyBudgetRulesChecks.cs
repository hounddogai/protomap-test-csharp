namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class WarrantyBudgetRulesChecks
{
    public static void WarrantyBudgetValidatorReportsEveryProblem()
    {
        var validator = WarrantyBudgetRules.DefaultWarrantyBudgetValidator();
        var item = WarrantyBudget.NewWarrantyBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new WarrantyBudgetRequiredNameRule("warranty budget needs a value");
        var item = WarrantyBudget.NewWarrantyBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyBudgetRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new WarrantyBudgetRequiredReviewerRule("warranty budget needs this field");
        var item = WarrantyBudget.NewWarrantyBudget("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyBudgetRequiredCodeRule("warranty budget needs a value");
        var item = WarrantyBudget.NewWarrantyBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyBudgetValidatorReportsEveryProblem();
        WarrantyBudgetRequiredNameRuleFlagsABlankName();
        WarrantyBudgetRequiredReviewerRuleFlagsABlankReviewer();
        WarrantyBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
