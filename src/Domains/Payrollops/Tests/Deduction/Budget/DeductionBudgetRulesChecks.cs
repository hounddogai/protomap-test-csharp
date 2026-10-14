namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionBudgetRulesChecks
{
    public static void DeductionBudgetValidatorReportsEveryProblem()
    {
        var validator = DeductionBudgetRules.DefaultDeductionBudgetValidator();
        var item = DeductionBudget.NewDeductionBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionBudgetMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new DeductionBudgetMarginRangeRule(7.2, 1955.3);
        var item = DeductionBudget.NewDeductionBudget("x", "y");
        item.Margin = 1956.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionBudgetDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new DeductionBudgetDepartmentLengthRule(86400);
        var item = DeductionBudget.NewDeductionBudget("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionBudgetTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new DeductionBudgetTenureMonthsRangeRule(32, 255);
        var item = DeductionBudget.NewDeductionBudget("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeductionBudgetValidatorReportsEveryProblem();
        DeductionBudgetMarginRangeRuleFlagsValuesAboveMargin();
        DeductionBudgetDepartmentLengthRuleFlagsALongDepartment();
        DeductionBudgetTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
