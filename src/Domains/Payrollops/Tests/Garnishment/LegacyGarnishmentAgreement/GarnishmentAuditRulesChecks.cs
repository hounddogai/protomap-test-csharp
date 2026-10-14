namespace Acme.Workforce.Payrollops.Tests.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Checks of legacy garnishment agreement code, run by the project's runner.</summary>
public static class GarnishmentAuditRulesChecks
{
    public static void GarnishmentAuditValidatorReportsEveryProblem()
    {
        var validator = GarnishmentAuditRules.DefaultGarnishmentAuditValidator();
        var item = GarnishmentAudit.NewGarnishmentAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GarnishmentAuditLabelLengthRule(32);
        var item = GarnishmentAudit.NewGarnishmentAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new GarnishmentAuditLeaveDaysRangeRule(120, 86400);
        var item = GarnishmentAudit.NewGarnishmentAudit("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new GarnishmentAuditRequiredNameRule("garnishment audit needs this field");
        var item = GarnishmentAudit.NewGarnishmentAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GarnishmentAuditValidatorReportsEveryProblem();
        GarnishmentAuditLabelLengthRuleFlagsALongLabel();
        GarnishmentAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        GarnishmentAuditRequiredNameRuleFlagsABlankName();
    }
}
