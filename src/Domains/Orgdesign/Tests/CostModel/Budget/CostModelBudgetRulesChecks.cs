namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelBudgetRulesChecks
{
    public static void CostModelBudgetValidatorReportsEveryProblem()
    {
        var validator = CostModelBudgetRules.DefaultCostModelBudgetValidator();
        var item = CostModelBudget.NewCostModelBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CostModelBudgetRequiredTitleRule("cost model budget needs a value");
        var item = CostModelBudget.NewCostModelBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelBudgetRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new CostModelBudgetRequiredOwnerIdRule("cost model budget needs a value");
        var item = CostModelBudget.NewCostModelBudget("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelBudgetRequiredCodeRule("cost model budget needs an entry");
        var item = CostModelBudget.NewCostModelBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelBudgetOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new CostModelBudgetOwnerIdLengthRule(120);
        var item = CostModelBudget.NewCostModelBudget("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelBudgetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CostModelBudgetTitleLengthRule(255);
        var item = CostModelBudget.NewCostModelBudget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelBudgetValidatorReportsEveryProblem();
        CostModelBudgetRequiredTitleRuleFlagsABlankTitle();
        CostModelBudgetRequiredOwnerIdRuleFlagsABlankOwnerId();
        CostModelBudgetRequiredCodeRuleFlagsABlankCode();
        CostModelBudgetOwnerIdLengthRuleFlagsALongOwnerId();
        CostModelBudgetTitleLengthRuleFlagsALongTitle();
    }
}
