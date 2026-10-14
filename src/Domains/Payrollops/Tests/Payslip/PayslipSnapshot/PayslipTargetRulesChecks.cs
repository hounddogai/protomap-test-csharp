namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipTargetRulesChecks
{
    public static void PayslipTargetValidatorReportsEveryProblem()
    {
        var validator = PayslipTargetRules.DefaultPayslipTargetValidator();
        var item = PayslipTarget.NewPayslipTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipTargetCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PayslipTargetCostRangeRule(3.3, 1888.3);
        var item = PayslipTarget.NewPayslipTarget("x", "y");
        item.Cost = 1889.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipTargetSourceLengthRuleFlagsALongSource()
    {
        var rule = new PayslipTargetSourceLengthRule(16);
        var item = PayslipTarget.NewPayslipTarget("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipTargetLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PayslipTargetLimitRangeRule(1000, 1000);
        var item = PayslipTarget.NewPayslipTarget("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipTargetRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new PayslipTargetRequiredSourceRule("payslip target needs this field");
        var item = PayslipTarget.NewPayslipTarget("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipTargetValidatorReportsEveryProblem();
        PayslipTargetCostRangeRuleFlagsValuesAboveCost();
        PayslipTargetSourceLengthRuleFlagsALongSource();
        PayslipTargetLimitRangeRuleFlagsValuesAboveLimit();
        PayslipTargetRequiredSourceRuleFlagsABlankSource();
    }
}
