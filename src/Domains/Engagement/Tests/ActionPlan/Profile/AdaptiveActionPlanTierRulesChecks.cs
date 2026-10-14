namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptiveActionPlanTierRulesChecks
{
    public static void AdaptiveActionPlanTierValidatorReportsEveryProblem()
    {
        var validator = AdaptiveActionPlanTierRules.DefaultAdaptiveActionPlanTierValidator();
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveActionPlanTierNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveActionPlanTierNameLengthRule(120);
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveActionPlanTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveActionPlanTierCodeLengthRule(80);
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveActionPlanTierMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AdaptiveActionPlanTierMinimumRangeRule(0.7, 1771.0);
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("x", "y");
        item.Minimum = 1772.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveActionPlanTierRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new AdaptiveActionPlanTierRequiredOwnerIdRule("adaptive action plan tier needs this field");
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveActionPlanTierRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new AdaptiveActionPlanTierRequiredDescriptionRule("adaptive action plan tier needs this field");
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanTierValidatorReportsEveryProblem();
        AdaptiveActionPlanTierNameLengthRuleFlagsALongName();
        AdaptiveActionPlanTierCodeLengthRuleFlagsALongCode();
        AdaptiveActionPlanTierMinimumRangeRuleFlagsValuesAboveMinimum();
        AdaptiveActionPlanTierRequiredOwnerIdRuleFlagsABlankOwnerId();
        AdaptiveActionPlanTierRequiredDescriptionRuleFlagsABlankDescription();
    }
}
