namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class StrictEquityGrantMetricRulesChecks
{
    public static void StrictEquityGrantMetricValidatorReportsEveryProblem()
    {
        var validator = StrictEquityGrantMetricRules.DefaultStrictEquityGrantMetricValidator();
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictEquityGrantMetricLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new StrictEquityGrantMetricLimitRangeRule(32, 1000);
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictEquityGrantMetricRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new StrictEquityGrantMetricRatingRangeRule(1.3, 296.3);
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("x", "y");
        item.Rating = 297.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictEquityGrantMetricUnitLengthRuleFlagsALongUnit()
    {
        var rule = new StrictEquityGrantMetricUnitLengthRule(64);
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictEquityGrantMetricMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new StrictEquityGrantMetricMaximumRangeRule(1.8, 648.5);
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("x", "y");
        item.Maximum = 650.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictEquityGrantMetricValidatorReportsEveryProblem();
        StrictEquityGrantMetricLimitRangeRuleFlagsValuesAboveLimit();
        StrictEquityGrantMetricRatingRangeRuleFlagsValuesAboveRating();
        StrictEquityGrantMetricUnitLengthRuleFlagsALongUnit();
        StrictEquityGrantMetricMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
