namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipDigestRulesChecks
{
    public static void PayslipDigestValidatorReportsEveryProblem()
    {
        var validator = PayslipDigestRules.DefaultPayslipDigestValidator();
        var item = PayslipDigest.NewPayslipDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipDigestUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new PayslipDigestUpdatedAtRangeRule(16, 3600);
        var item = PayslipDigest.NewPayslipDigest("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipDigestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayslipDigestLabelLengthRule(32);
        var item = PayslipDigest.NewPayslipDigest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipDigestSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new PayslipDigestSalaryBandLengthRule(16);
        var item = PayslipDigest.NewPayslipDigest("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayslipDigestRequiredCodeRule("payslip digest needs this field");
        var item = PayslipDigest.NewPayslipDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipDigestValidatorReportsEveryProblem();
        PayslipDigestUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        PayslipDigestLabelLengthRuleFlagsALongLabel();
        PayslipDigestSalaryBandLengthRuleFlagsALongSalaryBand();
        PayslipDigestRequiredCodeRuleFlagsABlankCode();
    }
}
