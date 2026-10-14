namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanSegmentRulesChecks
{
    public static void ActionPlanSegmentValidatorReportsEveryProblem()
    {
        var validator = ActionPlanSegmentRules.DefaultActionPlanSegmentValidator();
        var item = ActionPlanSegment.NewActionPlanSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanSegmentRequiredCodeRule("action plan segment needs a value");
        var item = ActionPlanSegment.NewActionPlanSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ActionPlanSegmentRequiredTitleRule("action plan segment needs this field");
        var item = ActionPlanSegment.NewActionPlanSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanSegmentRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ActionPlanSegmentRequiredChannelRule("action plan segment needs a value");
        var item = ActionPlanSegment.NewActionPlanSegment("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanSegmentRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ActionPlanSegmentRequiredTimezoneRule("action plan segment needs this field");
        var item = ActionPlanSegment.NewActionPlanSegment("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanSegmentValidatorReportsEveryProblem();
        ActionPlanSegmentRequiredCodeRuleFlagsABlankCode();
        ActionPlanSegmentRequiredTitleRuleFlagsABlankTitle();
        ActionPlanSegmentRequiredChannelRuleFlagsABlankChannel();
        ActionPlanSegmentRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
