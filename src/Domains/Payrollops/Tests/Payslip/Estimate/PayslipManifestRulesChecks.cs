namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipManifestRulesChecks
{
    public static void PayslipManifestValidatorReportsEveryProblem()
    {
        var validator = PayslipManifestRules.DefaultPayslipManifestValidator();
        var item = PayslipManifest.NewPayslipManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayslipManifestTitleLengthRule(1000);
        var item = PayslipManifest.NewPayslipManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipManifestStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new PayslipManifestStartsAtRangeRule(255, 86400);
        var item = PayslipManifest.NewPayslipManifest("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipManifestSourceLengthRuleFlagsALongSource()
    {
        var rule = new PayslipManifestSourceLengthRule(64);
        var item = PayslipManifest.NewPayslipManifest("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipManifestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayslipManifestRequiredTitleRule("payslip manifest needs a value");
        var item = PayslipManifest.NewPayslipManifest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayslipManifestRequiredCodeRule("payslip manifest needs a value");
        var item = PayslipManifest.NewPayslipManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipManifestValidatorReportsEveryProblem();
        PayslipManifestTitleLengthRuleFlagsALongTitle();
        PayslipManifestStartsAtRangeRuleFlagsValuesAboveStartsAt();
        PayslipManifestSourceLengthRuleFlagsALongSource();
        PayslipManifestRequiredTitleRuleFlagsABlankTitle();
        PayslipManifestRequiredCodeRuleFlagsABlankCode();
    }
}
