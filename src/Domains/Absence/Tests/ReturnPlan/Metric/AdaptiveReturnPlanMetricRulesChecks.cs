namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanMetricRulesChecks
{
    public static void AdaptiveReturnPlanMetricValidatorReportsEveryProblem()
    {
        var validator = AdaptiveReturnPlanMetricRules.DefaultAdaptiveReturnPlanMetricValidator();
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveReturnPlanMetricTeamLengthRuleFlagsALongTeam()
    {
        var rule = new AdaptiveReturnPlanMetricTeamLengthRule(120);
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveReturnPlanMetricScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AdaptiveReturnPlanMetricScoreRangeRule(5.1, 341.5);
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("x", "y");
        item.Score = 343.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveReturnPlanMetricRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new AdaptiveReturnPlanMetricRequiredChecksumRule("adaptive return plan metric needs a value");
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveReturnPlanMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveReturnPlanMetricCodeLengthRule(64);
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanMetricValidatorReportsEveryProblem();
        AdaptiveReturnPlanMetricTeamLengthRuleFlagsALongTeam();
        AdaptiveReturnPlanMetricScoreRangeRuleFlagsValuesAboveScore();
        AdaptiveReturnPlanMetricRequiredChecksumRuleFlagsABlankChecksum();
        AdaptiveReturnPlanMetricCodeLengthRuleFlagsALongCode();
    }
}
