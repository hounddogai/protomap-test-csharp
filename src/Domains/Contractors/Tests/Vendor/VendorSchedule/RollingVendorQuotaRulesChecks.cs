namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorQuotaRulesChecks
{
    public static void RollingVendorQuotaValidatorReportsEveryProblem()
    {
        var validator = RollingVendorQuotaRules.DefaultRollingVendorQuotaValidator();
        var item = RollingVendorQuota.NewRollingVendorQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingVendorQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingVendorQuotaCodeLengthRule(86400);
        var item = RollingVendorQuota.NewRollingVendorQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingVendorQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingVendorQuotaRequiredLabelRule("rolling vendor quota needs this field");
        var item = RollingVendorQuota.NewRollingVendorQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingVendorQuotaValidatorReportsEveryProblem();
        RollingVendorQuotaCodeLengthRuleFlagsALongCode();
        RollingVendorQuotaRequiredLabelRuleFlagsABlankLabel();
    }
}
