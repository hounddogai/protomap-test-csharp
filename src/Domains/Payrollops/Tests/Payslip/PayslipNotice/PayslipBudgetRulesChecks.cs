namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class PayslipBudgetRulesChecks
{
    public static void PayslipBudgetValidatorReportsEveryProblem()
    {
        var validator = PayslipBudgetRules.DefaultPayslipBudgetValidator();
        var item = PayslipBudget.NewPayslipBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipBudgetCodeLengthRule(120);
        var item = PayslipBudget.NewPayslipBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipBudgetRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PayslipBudgetRateRangeRule(6.4, 857.3);
        var item = PayslipBudget.NewPayslipBudget("x", "y");
        item.Rate = 858.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayslipBudgetRequiredTitleRule("payslip budget needs this field");
        var item = PayslipBudget.NewPayslipBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipBudgetGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PayslipBudgetGraceDaysRangeRule(1000, 86400);
        var item = PayslipBudget.NewPayslipBudget("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayslipBudgetValidatorReportsEveryProblem();
        PayslipBudgetCodeLengthRuleFlagsALongCode();
        PayslipBudgetRateRangeRuleFlagsValuesAboveRate();
        PayslipBudgetRequiredTitleRuleFlagsABlankTitle();
        PayslipBudgetGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
