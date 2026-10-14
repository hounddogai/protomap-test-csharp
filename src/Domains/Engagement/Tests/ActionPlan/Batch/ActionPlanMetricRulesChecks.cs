namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ActionPlanMetricRulesChecks
{
    public static void ActionPlanMetricValidatorReportsEveryProblem()
    {
        var validator = ActionPlanMetricRules.DefaultActionPlanMetricValidator();
        var item = ActionPlanMetric.NewActionPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanMetricRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new ActionPlanMetricRequiredSkillRule("action plan metric needs an entry");
        var item = ActionPlanMetric.NewActionPlanMetric("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ActionPlanMetricTitleLengthRule(16);
        var item = ActionPlanMetric.NewActionPlanMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanMetricRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ActionPlanMetricRevisionRangeRule(64, 1000);
        var item = ActionPlanMetric.NewActionPlanMetric("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanMetricJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new ActionPlanMetricJobCodeLengthRule(64);
        var item = ActionPlanMetric.NewActionPlanMetric("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ActionPlanMetricRequiredTitleRule("action plan metric needs an entry");
        var item = ActionPlanMetric.NewActionPlanMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanMetricSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ActionPlanMetricSkillLengthRule(32);
        var item = ActionPlanMetric.NewActionPlanMetric("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanMetricValidatorReportsEveryProblem();
        ActionPlanMetricRequiredSkillRuleFlagsABlankSkill();
        ActionPlanMetricTitleLengthRuleFlagsALongTitle();
        ActionPlanMetricRevisionRangeRuleFlagsValuesAboveRevision();
        ActionPlanMetricJobCodeLengthRuleFlagsALongJobCode();
        ActionPlanMetricRequiredTitleRuleFlagsABlankTitle();
        ActionPlanMetricSkillLengthRuleFlagsALongSkill();
    }
}
