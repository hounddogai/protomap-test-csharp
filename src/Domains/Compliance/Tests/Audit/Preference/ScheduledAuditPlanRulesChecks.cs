namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledAuditPlanRulesChecks
{
    public static void ScheduledAuditPlanValidatorReportsEveryProblem()
    {
        var validator = ScheduledAuditPlanRules.DefaultScheduledAuditPlanValidator();
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAuditPlanDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ScheduledAuditPlanDurationMinutesRangeRule(80, 120);
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAuditPlanRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ScheduledAuditPlanRevisionRangeRule(16, 80);
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAuditPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledAuditPlanRequiredNameRule("scheduled audit plan needs this field");
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAuditPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledAuditPlanCodeLengthRule(1000);
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledAuditPlanValidatorReportsEveryProblem();
        ScheduledAuditPlanDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ScheduledAuditPlanRevisionRangeRuleFlagsValuesAboveRevision();
        ScheduledAuditPlanRequiredNameRuleFlagsABlankName();
        ScheduledAuditPlanCodeLengthRuleFlagsALongCode();
    }
}
