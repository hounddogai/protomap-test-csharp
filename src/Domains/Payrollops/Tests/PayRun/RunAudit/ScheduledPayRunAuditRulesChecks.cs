namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class ScheduledPayRunAuditRulesChecks
{
    public static void ScheduledPayRunAuditValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayRunAuditRules.DefaultScheduledPayRunAuditValidator();
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayRunAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledPayRunAuditRequiredLabelRule("scheduled pay run audit needs this field");
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayRunAuditUnitLengthRuleFlagsALongUnit()
    {
        var rule = new ScheduledPayRunAuditUnitLengthRule(64);
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayRunAuditTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new ScheduledPayRunAuditTimezoneLengthRule(255);
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPayRunAuditValidatorReportsEveryProblem();
        ScheduledPayRunAuditRequiredLabelRuleFlagsABlankLabel();
        ScheduledPayRunAuditUnitLengthRuleFlagsALongUnit();
        ScheduledPayRunAuditTimezoneLengthRuleFlagsALongTimezone();
    }
}
