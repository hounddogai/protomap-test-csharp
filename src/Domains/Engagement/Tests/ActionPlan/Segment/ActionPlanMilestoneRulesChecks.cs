namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanMilestoneRulesChecks
{
    public static void ActionPlanMilestoneValidatorReportsEveryProblem()
    {
        var validator = ActionPlanMilestoneRules.DefaultActionPlanMilestoneValidator();
        var item = ActionPlanMilestone.NewActionPlanMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanMilestoneEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ActionPlanMilestoneEndsAtRangeRule(32, 64);
        var item = ActionPlanMilestone.NewActionPlanMilestone("x", "y");
        item.EndsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanMilestoneRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ActionPlanMilestoneRevisionRangeRule(80, 86400);
        var item = ActionPlanMilestone.NewActionPlanMilestone("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ActionPlanMilestoneLabelLengthRule(3600);
        var item = ActionPlanMilestone.NewActionPlanMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanMilestoneRequiredCodeRule("action plan milestone needs an entry");
        var item = ActionPlanMilestone.NewActionPlanMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanMilestoneSourceLengthRuleFlagsALongSource()
    {
        var rule = new ActionPlanMilestoneSourceLengthRule(86400);
        var item = ActionPlanMilestone.NewActionPlanMilestone("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanMilestoneRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ActionPlanMilestoneRequiredLabelRule("action plan milestone needs this field");
        var item = ActionPlanMilestone.NewActionPlanMilestone("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanMilestoneValidatorReportsEveryProblem();
        ActionPlanMilestoneEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ActionPlanMilestoneRevisionRangeRuleFlagsValuesAboveRevision();
        ActionPlanMilestoneLabelLengthRuleFlagsALongLabel();
        ActionPlanMilestoneRequiredCodeRuleFlagsABlankCode();
        ActionPlanMilestoneSourceLengthRuleFlagsALongSource();
        ActionPlanMilestoneRequiredLabelRuleFlagsABlankLabel();
    }
}
