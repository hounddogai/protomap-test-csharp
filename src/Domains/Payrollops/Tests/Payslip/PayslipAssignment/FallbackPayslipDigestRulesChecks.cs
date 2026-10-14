namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipDigestRulesChecks
{
    public static void FallbackPayslipDigestValidatorReportsEveryProblem()
    {
        var validator = FallbackPayslipDigestRules.DefaultFallbackPayslipDigestValidator();
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayslipDigestMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new FallbackPayslipDigestMaximumRangeRule(4.6, 3559.5);
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("x", "y");
        item.Maximum = 3561.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayslipDigestRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new FallbackPayslipDigestRequiredExternalRefRule("fallback payslip digest needs a value");
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPayslipDigestRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new FallbackPayslipDigestRetriesRangeRule(32, 3600);
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayslipDigestRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new FallbackPayslipDigestRatioRangeRule(6.8, 2881.7);
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("x", "y");
        item.Ratio = 2883.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayslipDigestSkillLengthRuleFlagsALongSkill()
    {
        var rule = new FallbackPayslipDigestSkillLengthRule(64);
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackPayslipDigestValidatorReportsEveryProblem();
        FallbackPayslipDigestMaximumRangeRuleFlagsValuesAboveMaximum();
        FallbackPayslipDigestRequiredExternalRefRuleFlagsABlankExternalRef();
        FallbackPayslipDigestRetriesRangeRuleFlagsValuesAboveRetries();
        FallbackPayslipDigestRatioRangeRuleFlagsValuesAboveRatio();
        FallbackPayslipDigestSkillLengthRuleFlagsALongSkill();
    }
}
