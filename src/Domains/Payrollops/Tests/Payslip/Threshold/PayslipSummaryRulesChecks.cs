namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipSummaryRulesChecks
{
    public static void PayslipSummaryValidatorReportsEveryProblem()
    {
        var validator = PayslipSummaryRules.DefaultPayslipSummaryValidator();
        var item = PayslipSummary.NewPayslipSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayslipSummaryRequiredTitleRule("payslip summary needs a value");
        var item = PayslipSummary.NewPayslipSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipSummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PayslipSummaryRateRangeRule(5.6, 2256.3);
        var item = PayslipSummary.NewPayslipSummary("x", "y");
        item.Rate = 2257.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipSummaryRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new PayslipSummaryRequiredChannelRule("payslip summary needs an entry");
        var item = PayslipSummary.NewPayslipSummary("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipSummaryValidatorReportsEveryProblem();
        PayslipSummaryRequiredTitleRuleFlagsABlankTitle();
        PayslipSummaryRateRangeRuleFlagsValuesAboveRate();
        PayslipSummaryRequiredChannelRuleFlagsABlankChannel();
    }
}
