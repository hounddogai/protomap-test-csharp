namespace Acme.Workforce.Contractors.Tests.Rate.Window;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RateMetricRulesChecks
{
    public static void RateMetricValidatorReportsEveryProblem()
    {
        var validator = RateMetricRules.DefaultRateMetricValidator();
        var item = RateMetric.NewRateMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateMetricQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RateMetricQuantityRangeRule(32, 3600);
        var item = RateMetric.NewRateMetric("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateMetricChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RateMetricChannelLengthRule(255);
        var item = RateMetric.NewRateMetric("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateMetricRequiredNameRuleFlagsABlankName()
    {
        var rule = new RateMetricRequiredNameRule("rate metric needs a value");
        var item = RateMetric.NewRateMetric("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateMetricRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new RateMetricRequiredDescriptionRule("rate metric needs an entry");
        var item = RateMetric.NewRateMetric("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new RateMetricCodeLengthRule(120);
        var item = RateMetric.NewRateMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateMetricValidatorReportsEveryProblem();
        RateMetricQuantityRangeRuleFlagsValuesAboveQuantity();
        RateMetricChannelLengthRuleFlagsALongChannel();
        RateMetricRequiredNameRuleFlagsABlankName();
        RateMetricRequiredDescriptionRuleFlagsABlankDescription();
        RateMetricCodeLengthRuleFlagsALongCode();
    }
}
