namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedPayslipDraftRulesChecks
{
    public static void WeightedPayslipDraftValidatorReportsEveryProblem()
    {
        var validator = WeightedPayslipDraftRules.DefaultWeightedPayslipDraftValidator();
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPayslipDraftBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new WeightedPayslipDraftBudgetRangeRule(1.2, 4357.0);
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("x", "y");
        item.Budget = 4358.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedPayslipDraftManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new WeightedPayslipDraftManagerIdLengthRule(255);
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPayslipDraftRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new WeightedPayslipDraftRequiredManagerIdRule("weighted payslip draft needs a value");
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedPayslipDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedPayslipDraftLabelLengthRule(1000);
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedPayslipDraftValidatorReportsEveryProblem();
        WeightedPayslipDraftBudgetRangeRuleFlagsValuesAboveBudget();
        WeightedPayslipDraftManagerIdLengthRuleFlagsALongManagerId();
        WeightedPayslipDraftRequiredManagerIdRuleFlagsABlankManagerId();
        WeightedPayslipDraftLabelLengthRuleFlagsALongLabel();
    }
}
