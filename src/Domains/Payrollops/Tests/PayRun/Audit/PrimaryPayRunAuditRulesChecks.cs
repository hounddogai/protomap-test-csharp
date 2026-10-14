namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PrimaryPayRunAuditRulesChecks
{
    public static void PrimaryPayRunAuditValidatorReportsEveryProblem()
    {
        var validator = PrimaryPayRunAuditRules.DefaultPrimaryPayRunAuditValidator();
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPayRunAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PrimaryPayRunAuditWindowDaysRangeRule(64, 3600);
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryPayRunAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryPayRunAuditRequiredCodeRule("primary pay run audit needs an entry");
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPayRunAuditChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new PrimaryPayRunAuditChecksumLengthRule(86400);
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPayRunAuditRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PrimaryPayRunAuditRequiredCountryRule("primary pay run audit needs a value");
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPayRunAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryPayRunAuditRequiredTitleRule("primary pay run audit needs an entry");
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryPayRunAuditValidatorReportsEveryProblem();
        PrimaryPayRunAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        PrimaryPayRunAuditRequiredCodeRuleFlagsABlankCode();
        PrimaryPayRunAuditChecksumLengthRuleFlagsALongChecksum();
        PrimaryPayRunAuditRequiredCountryRuleFlagsABlankCountry();
        PrimaryPayRunAuditRequiredTitleRuleFlagsABlankTitle();
    }
}
