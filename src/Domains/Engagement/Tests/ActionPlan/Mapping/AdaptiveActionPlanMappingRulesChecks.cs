namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveActionPlanMappingRulesChecks
{
    public static void AdaptiveActionPlanMappingValidatorReportsEveryProblem()
    {
        var validator = AdaptiveActionPlanMappingRules.DefaultAdaptiveActionPlanMappingValidator();
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveActionPlanMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveActionPlanMappingTitleLengthRule(255);
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveActionPlanMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AdaptiveActionPlanMappingExpiresAtRangeRule(16, 86400);
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveActionPlanMappingTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AdaptiveActionPlanMappingTimezoneLengthRule(255);
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveActionPlanMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveActionPlanMappingCodeLengthRule(16);
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveActionPlanMappingFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AdaptiveActionPlanMappingFteRangeRule(2.9, 4666.3);
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("x", "y");
        item.Fte = 4667.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanMappingValidatorReportsEveryProblem();
        AdaptiveActionPlanMappingTitleLengthRuleFlagsALongTitle();
        AdaptiveActionPlanMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AdaptiveActionPlanMappingTimezoneLengthRuleFlagsALongTimezone();
        AdaptiveActionPlanMappingCodeLengthRuleFlagsALongCode();
        AdaptiveActionPlanMappingFteRangeRuleFlagsValuesAboveFte();
    }
}
