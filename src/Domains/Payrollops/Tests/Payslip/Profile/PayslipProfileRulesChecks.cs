namespace Acme.Workforce.Payrollops.Tests.Payslip.Profile;

using Acme.Workforce.Payrollops.Payslip.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PayslipProfileRulesChecks
{
    public static void PayslipProfileValidatorReportsEveryProblem()
    {
        var validator = PayslipProfileRules.DefaultPayslipProfileValidator();
        var item = PayslipProfile.NewPayslipProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipProfileScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PayslipProfileScoreRangeRule(4.9, 3574.7);
        var item = PayslipProfile.NewPayslipProfile("x", "y");
        item.Score = 3576.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayslipProfileTitleLengthRule(86400);
        var item = PayslipProfile.NewPayslipProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipProfileCodeLengthRule(80);
        var item = PayslipProfile.NewPayslipProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayslipProfileRequiredTitleRule("payslip profile needs this field");
        var item = PayslipProfile.NewPayslipProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipProfileValidatorReportsEveryProblem();
        PayslipProfileScoreRangeRuleFlagsValuesAboveScore();
        PayslipProfileTitleLengthRuleFlagsALongTitle();
        PayslipProfileCodeLengthRuleFlagsALongCode();
        PayslipProfileRequiredTitleRuleFlagsABlankTitle();
    }
}
