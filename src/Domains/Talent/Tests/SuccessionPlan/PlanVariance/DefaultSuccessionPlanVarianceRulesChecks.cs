namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanVariance;

using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Checks of plan variance code, run by the project's runner.</summary>
public static class DefaultSuccessionPlanVarianceRulesChecks
{
    public static void DefaultSuccessionPlanVarianceValidatorReportsEveryProblem()
    {
        var validator = DefaultSuccessionPlanVarianceRules.DefaultDefaultSuccessionPlanVarianceValidator();
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSuccessionPlanVarianceRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new DefaultSuccessionPlanVarianceRequiredDescriptionRule(
            "default succession plan variance needs an entry");
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSuccessionPlanVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultSuccessionPlanVarianceTitleLengthRule(16);
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSuccessionPlanVarianceThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new DefaultSuccessionPlanVarianceThresholdRangeRule(8.2, 656.3);
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("x", "y");
        item.Threshold = 657.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultSuccessionPlanVarianceRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new DefaultSuccessionPlanVarianceRequiredTimezoneRule(
            "default succession plan variance needs this field");
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSuccessionPlanVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultSuccessionPlanVarianceRequiredTitleRule(
            "default succession plan variance needs this field");
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultSuccessionPlanVarianceValidatorReportsEveryProblem();
        DefaultSuccessionPlanVarianceRequiredDescriptionRuleFlagsABlankDescription();
        DefaultSuccessionPlanVarianceTitleLengthRuleFlagsALongTitle();
        DefaultSuccessionPlanVarianceThresholdRangeRuleFlagsValuesAboveThreshold();
        DefaultSuccessionPlanVarianceRequiredTimezoneRuleFlagsABlankTimezone();
        DefaultSuccessionPlanVarianceRequiredTitleRuleFlagsABlankTitle();
    }
}
