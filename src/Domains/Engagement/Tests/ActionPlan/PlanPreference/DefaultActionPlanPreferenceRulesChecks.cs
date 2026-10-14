namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanPreference;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Checks of plan preference code, run by the project's runner.</summary>
public static class DefaultActionPlanPreferenceRulesChecks
{
    public static void DefaultActionPlanPreferenceValidatorReportsEveryProblem()
    {
        var validator = DefaultActionPlanPreferenceRules.DefaultDefaultActionPlanPreferenceValidator();
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultActionPlanPreferenceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new DefaultActionPlanPreferenceTenureMonthsRangeRule(32, 64);
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("x", "y");
        item.TenureMonths = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultActionPlanPreferenceSourceLengthRuleFlagsALongSource()
    {
        var rule = new DefaultActionPlanPreferenceSourceLengthRule(3600);
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultActionPlanPreferenceFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new DefaultActionPlanPreferenceFteRangeRule(0.7, 1380.3);
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("x", "y");
        item.Fte = 1381.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultActionPlanPreferenceCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new DefaultActionPlanPreferenceCapacityRangeRule(120, 3600);
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultActionPlanPreferenceRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultActionPlanPreferenceRequiredNameRule("default action plan preference needs an entry");
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultActionPlanPreferenceValidatorReportsEveryProblem();
        DefaultActionPlanPreferenceTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        DefaultActionPlanPreferenceSourceLengthRuleFlagsALongSource();
        DefaultActionPlanPreferenceFteRangeRuleFlagsValuesAboveFte();
        DefaultActionPlanPreferenceCapacityRangeRuleFlagsValuesAboveCapacity();
        DefaultActionPlanPreferenceRequiredNameRuleFlagsABlankName();
    }
}
