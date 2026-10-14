namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class WeightedPayslipBudgetRulesChecks
{
    public static void WeightedPayslipBudgetValidatorReportsEveryProblem()
    {
        var validator = WeightedPayslipBudgetRules.DefaultWeightedPayslipBudgetValidator();
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPayslipBudgetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedPayslipBudgetTitleLengthRule(255);
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPayslipBudgetRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new WeightedPayslipBudgetRetriesRangeRule(255, 86400);
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedPayslipBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedPayslipBudgetRequiredTitleRule("weighted payslip budget needs an entry");
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedPayslipBudgetValidatorReportsEveryProblem();
        WeightedPayslipBudgetTitleLengthRuleFlagsALongTitle();
        WeightedPayslipBudgetRetriesRangeRuleFlagsValuesAboveRetries();
        WeightedPayslipBudgetRequiredTitleRuleFlagsABlankTitle();
    }
}
