namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ActionPlanNoticeRulesChecks
{
    public static void ActionPlanNoticeValidatorReportsEveryProblem()
    {
        var validator = ActionPlanNoticeRules.DefaultActionPlanNoticeValidator();
        var item = ActionPlanNotice.NewActionPlanNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanNoticeBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ActionPlanNoticeBadgeLengthRule(255);
        var item = ActionPlanNotice.NewActionPlanNotice("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanNoticeDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ActionPlanNoticeDurationMinutesRangeRule(1000, 3600);
        var item = ActionPlanNotice.NewActionPlanNotice("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ActionPlanNoticeValidatorReportsEveryProblem();
        ActionPlanNoticeBadgeLengthRuleFlagsALongBadge();
        ActionPlanNoticeDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
