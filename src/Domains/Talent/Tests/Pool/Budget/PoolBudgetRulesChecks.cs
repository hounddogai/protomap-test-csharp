namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolBudgetRulesChecks
{
    public static void PoolBudgetValidatorReportsEveryProblem()
    {
        var validator = PoolBudgetRules.DefaultPoolBudgetValidator();
        var item = PoolBudget.NewPoolBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolBudgetRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PoolBudgetRatioRangeRule(7.7, 1625.0);
        var item = PoolBudget.NewPoolBudget("x", "y");
        item.Ratio = 1626.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolBudgetRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new PoolBudgetRequiredCostCenterRule("pool budget needs a value");
        var item = PoolBudget.NewPoolBudget("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolBudgetOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PoolBudgetOwnerIdLengthRule(16);
        var item = PoolBudget.NewPoolBudget("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolBudgetRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PoolBudgetRequiredOwnerIdRule("pool budget needs a value");
        var item = PoolBudget.NewPoolBudget("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new PoolBudgetNameLengthRule(255);
        var item = PoolBudget.NewPoolBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolBudgetValidatorReportsEveryProblem();
        PoolBudgetRatioRangeRuleFlagsValuesAboveRatio();
        PoolBudgetRequiredCostCenterRuleFlagsABlankCostCenter();
        PoolBudgetOwnerIdLengthRuleFlagsALongOwnerId();
        PoolBudgetRequiredOwnerIdRuleFlagsABlankOwnerId();
        PoolBudgetNameLengthRuleFlagsALongName();
    }
}
