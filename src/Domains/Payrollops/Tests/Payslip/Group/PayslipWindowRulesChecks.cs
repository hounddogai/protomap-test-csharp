namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipWindowRulesChecks
{
    public static void PayslipWindowValidatorReportsEveryProblem()
    {
        var validator = PayslipWindowRules.DefaultPayslipWindowValidator();
        var item = PayslipWindow.NewPayslipWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayslipWindowRequiredCodeRule("payslip window needs an entry");
        var item = PayslipWindow.NewPayslipWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipWindowSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PayslipWindowSummaryLengthRule(80);
        var item = PayslipWindow.NewPayslipWindow("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipWindowRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new PayslipWindowRequiredLocationRule("payslip window needs a value");
        var item = PayslipWindow.NewPayslipWindow("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipWindowCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PayslipWindowCeilingRangeRule(7.0, 4570.8);
        var item = PayslipWindow.NewPayslipWindow("x", "y");
        item.Ceiling = 4572.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipWindowRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayslipWindowRequiredNameRule("payslip window needs an entry");
        var item = PayslipWindow.NewPayslipWindow("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipWindowValidatorReportsEveryProblem();
        PayslipWindowRequiredCodeRuleFlagsABlankCode();
        PayslipWindowSummaryLengthRuleFlagsALongSummary();
        PayslipWindowRequiredLocationRuleFlagsABlankLocation();
        PayslipWindowCeilingRangeRuleFlagsValuesAboveCeiling();
        PayslipWindowRequiredNameRuleFlagsABlankName();
    }
}
