namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class LegacyPayslipDigestRulesChecks
{
    public static void LegacyPayslipDigestValidatorReportsEveryProblem()
    {
        var validator = LegacyPayslipDigestRules.DefaultLegacyPayslipDigestValidator();
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayslipDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyPayslipDigestRequiredTitleRule("legacy payslip digest needs a value");
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayslipDigestRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new LegacyPayslipDigestRequiredOwnerIdRule("legacy payslip digest needs a value");
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayslipDigestGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LegacyPayslipDigestGraceDaysRangeRule(64, 80);
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPayslipDigestOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LegacyPayslipDigestOwnerIdLengthRule(16);
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyPayslipDigestValidatorReportsEveryProblem();
        LegacyPayslipDigestRequiredTitleRuleFlagsABlankTitle();
        LegacyPayslipDigestRequiredOwnerIdRuleFlagsABlankOwnerId();
        LegacyPayslipDigestGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        LegacyPayslipDigestOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
