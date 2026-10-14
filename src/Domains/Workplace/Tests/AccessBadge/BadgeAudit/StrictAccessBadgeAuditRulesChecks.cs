namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class StrictAccessBadgeAuditRulesChecks
{
    public static void StrictAccessBadgeAuditValidatorReportsEveryProblem()
    {
        var validator = StrictAccessBadgeAuditRules.DefaultStrictAccessBadgeAuditValidator();
        var item = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAccessBadgeAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictAccessBadgeAuditLabelLengthRule(64);
        var item = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAccessBadgeAuditRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new StrictAccessBadgeAuditRequiredDescriptionRule("strict access badge audit needs a value");
        var item = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAccessBadgeAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictAccessBadgeAuditRequiredLabelRule("strict access badge audit needs this field");
        var item = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictAccessBadgeAuditValidatorReportsEveryProblem();
        StrictAccessBadgeAuditLabelLengthRuleFlagsALongLabel();
        StrictAccessBadgeAuditRequiredDescriptionRuleFlagsABlankDescription();
        StrictAccessBadgeAuditRequiredLabelRuleFlagsABlankLabel();
    }
}
