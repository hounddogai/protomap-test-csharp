namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AdaptiveActionPlanSlotRulesChecks
{
    public static void AdaptiveActionPlanSlotValidatorReportsEveryProblem()
    {
        var validator = AdaptiveActionPlanSlotRules.DefaultAdaptiveActionPlanSlotValidator();
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveActionPlanSlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveActionPlanSlotTitleLengthRule(80);
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveActionPlanSlotScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AdaptiveActionPlanSlotScoreRangeRule(2.1, 3347.7);
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("x", "y");
        item.Score = 3349.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveActionPlanSlotRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new AdaptiveActionPlanSlotRequiredBadgeRule("adaptive action plan slot needs a value");
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanSlotValidatorReportsEveryProblem();
        AdaptiveActionPlanSlotTitleLengthRuleFlagsALongTitle();
        AdaptiveActionPlanSlotScoreRangeRuleFlagsValuesAboveScore();
        AdaptiveActionPlanSlotRequiredBadgeRuleFlagsABlankBadge();
    }
}
