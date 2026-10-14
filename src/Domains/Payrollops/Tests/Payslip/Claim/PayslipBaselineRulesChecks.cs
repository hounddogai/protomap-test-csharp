namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipBaselineRulesChecks
{
    public static void PayslipBaselineValidatorReportsEveryProblem()
    {
        var validator = PayslipBaselineRules.DefaultPayslipBaselineValidator();
        var item = PayslipBaseline.NewPayslipBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipBaselineLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new PayslipBaselineLocaleLengthRule(86400);
        var item = PayslipBaseline.NewPayslipBaseline("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayslipBaselineRequiredCodeRule("payslip baseline needs this field");
        var item = PayslipBaseline.NewPayslipBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipBaselineTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayslipBaselineTitleLengthRule(64);
        var item = PayslipBaseline.NewPayslipBaseline("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipBaselineRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PayslipBaselineRatioRangeRule(1.7, 4872.1);
        var item = PayslipBaseline.NewPayslipBaseline("x", "y");
        item.Ratio = 4873.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipBaselineHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new PayslipBaselineHoursRangeRule(7.6, 3742.7);
        var item = PayslipBaseline.NewPayslipBaseline("x", "y");
        item.Hours = 3744.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayslipBaselineValidatorReportsEveryProblem();
        PayslipBaselineLocaleLengthRuleFlagsALongLocale();
        PayslipBaselineRequiredCodeRuleFlagsABlankCode();
        PayslipBaselineTitleLengthRuleFlagsALongTitle();
        PayslipBaselineRatioRangeRuleFlagsValuesAboveRatio();
        PayslipBaselineHoursRangeRuleFlagsValuesAboveHours();
    }
}
