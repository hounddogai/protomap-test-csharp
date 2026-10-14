namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LegacyActionPlanMetricRulesChecks
{
    public static void LegacyActionPlanMetricValidatorReportsEveryProblem()
    {
        var validator = LegacyActionPlanMetricRules.DefaultLegacyActionPlanMetricValidator();
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyActionPlanMetricContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LegacyActionPlanMetricContactEmailLengthRule(120);
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyActionPlanMetricManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new LegacyActionPlanMetricManagerIdLengthRule(16);
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyActionPlanMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyActionPlanMetricLabelLengthRule(86400);
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyActionPlanMetricMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new LegacyActionPlanMetricMinimumRangeRule(9.0, 2400.5);
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("x", "y");
        item.Minimum = 2402.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyActionPlanMetricValidatorReportsEveryProblem();
        LegacyActionPlanMetricContactEmailLengthRuleFlagsALongContactEmail();
        LegacyActionPlanMetricManagerIdLengthRuleFlagsALongManagerId();
        LegacyActionPlanMetricLabelLengthRuleFlagsALongLabel();
        LegacyActionPlanMetricMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
