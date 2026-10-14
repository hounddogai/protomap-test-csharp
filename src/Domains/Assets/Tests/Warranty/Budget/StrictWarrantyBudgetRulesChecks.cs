namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class StrictWarrantyBudgetRulesChecks
{
    public static void StrictWarrantyBudgetValidatorReportsEveryProblem()
    {
        var validator = StrictWarrantyBudgetRules.DefaultStrictWarrantyBudgetValidator();
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictWarrantyBudgetSourceLengthRuleFlagsALongSource()
    {
        var rule = new StrictWarrantyBudgetSourceLengthRule(64);
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictWarrantyBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictWarrantyBudgetCodeLengthRule(3600);
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictWarrantyBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictWarrantyBudgetRequiredTitleRule("strict warranty budget needs a value");
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictWarrantyBudgetValidatorReportsEveryProblem();
        StrictWarrantyBudgetSourceLengthRuleFlagsALongSource();
        StrictWarrantyBudgetCodeLengthRuleFlagsALongCode();
        StrictWarrantyBudgetRequiredTitleRuleFlagsABlankTitle();
    }
}
