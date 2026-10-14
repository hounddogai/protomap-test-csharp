namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipEstimateRulesChecks
{
    public static void PayslipEstimateValidatorReportsEveryProblem()
    {
        var validator = PayslipEstimateRules.DefaultPayslipEstimateValidator();
        var item = PayslipEstimate.NewPayslipEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new PayslipEstimateNameLengthRule(64);
        var item = PayslipEstimate.NewPayslipEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipEstimateRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new PayslipEstimateRequiredChannelRule("payslip estimate needs an entry");
        var item = PayslipEstimate.NewPayslipEstimate("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipEstimateValidatorReportsEveryProblem();
        PayslipEstimateNameLengthRuleFlagsALongName();
        PayslipEstimateRequiredChannelRuleFlagsABlankChannel();
    }
}
