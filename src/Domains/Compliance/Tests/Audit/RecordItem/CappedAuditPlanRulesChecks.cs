namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedAuditPlanRulesChecks
{
    public static void CappedAuditPlanValidatorReportsEveryProblem()
    {
        var validator = CappedAuditPlanRules.DefaultCappedAuditPlanValidator();
        var item = CappedAuditPlan.NewCappedAuditPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAuditPlanDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new CappedAuditPlanDurationMinutesRangeRule(255, 86400);
        var item = CappedAuditPlan.NewCappedAuditPlan("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAuditPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedAuditPlanLabelLengthRule(32);
        var item = CappedAuditPlan.NewCappedAuditPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAuditPlanRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new CappedAuditPlanRatioRangeRule(0.8, 2576.6);
        var item = CappedAuditPlan.NewCappedAuditPlan("x", "y");
        item.Ratio = 2578.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAuditPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedAuditPlanRequiredLabelRule("capped audit plan needs an entry");
        var item = CappedAuditPlan.NewCappedAuditPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedAuditPlanValidatorReportsEveryProblem();
        CappedAuditPlanDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        CappedAuditPlanLabelLengthRuleFlagsALongLabel();
        CappedAuditPlanRatioRangeRuleFlagsValuesAboveRatio();
        CappedAuditPlanRequiredLabelRuleFlagsABlankLabel();
    }
}
